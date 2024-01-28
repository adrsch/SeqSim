using BulletSharp;
using Stride.Animations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;
using SEQ.Sim;

namespace SEQ.Sim
{
    [Serializable]
    public class ProjectileManager
    {
        public void DoInit()
        {
          //  Pool = new PrefabPool<Projectile>(Prefab.gameObject, SharedPools.Inst.transform);
        }
        public Weapon Weapon;
        public ActorState Owner;

        public IUsableUser User;
        public ActorSpecies Species => Weapon.Species;
        public void ResetWeapon()
        {
            /*
            if (Pool != null)
                Pool.Clear();
            */
        }

        public static int currentShotPos;
        float lastShotTime;


        public event Action OnShotsFired;

        public void Launch(TransformComponent muzzle, Vector3 p, Vector3 forward)
        {
            /*var proj = Pool.Get();
            proj.DoReset();
            proj.RecycleAction = () => Pool.Recycle(proj.gameObject);
            proj.Launch(muzzle, real);
            */
            if (Weapon.Animator.MuzzleEffect != null)
            {
                var muzzlefx = Weapon.Animator.MuzzleEffect.InstantiateTemporary(muzzle.Entity.Scene, 100);
                muzzlefx.SetParentAndZero(muzzle.Transform.Entity);
            }
            if (Species.IsRaycast)
            {
                DoRaycast(p, forward);
            }
            else
            {
                //todo
            }

            OnShotsFired?.Invoke();
        }

        List<HitResult> Hits = new List<HitResult>();


        void DoRaycast(Vector3 start, Vector3 forward)
        {
            var keyframes = (Weapon.Species.SpreadCurve.Curve as ComputeAnimationCurveVector2).KeyFrames;
            if (Time.time > lastShotTime + 0.6f || currentShotPos >= keyframes.Count)
            {
                currentShotPos = 0;
            }
            else if (Time.time > lastShotTime + 0.3f)
            {

                currentShotPos -= 2;
                if (currentShotPos < 0) currentShotPos = 0;
                if (currentShotPos > 4) currentShotPos = 4;
            }
            // var key = Weapon.Species.SpreadCurve.KeyFrames[currentShotPos];
            var spread = User.GetSpread();
            var spraySample = keyframes[currentShotPos].Value * 0.15f * spread;
            currentShotPos++;
            lastShotTime = Time.time;
            //Logger.Log(Channel.Gameplay, LogPriority.Info, $"pos {currentShotPos} sample {spraySample.X} {spraySample.Y}");
            //Vector2 spraySample = Weapon.Species.SpreadCurveX.Keys()


            forward = Quaternion.RotationX(spraySample.X) * Quaternion.RotationY(spraySample.Y) * forward;
          //  var spread = User.GetSpread();
           // var rotationmatrix = Quaternion.a
            //forward = Qua
            var end = start + forward * Weapon.Species.MaxRange;

            var tryagain = true;
            var penetrationLayer = 0;

            var damageRemaining = 1f;
            Vector3 lastPenetrationPosition = start;

            Hits.Clear();
            InteractionProbe.S.GetSimulation().RaycastPenetrating(start, end, Hits);

            //Hits.Sort((x, y) => Vector3.Distance(x.Point, start).CompareTo(Vector3.Distance(y.Point, start)));
           Hits.Sort((x, y) => x.HitFraction.CompareTo(y.HitFraction));

            foreach (var result in Hits)
           // while (tryagain && counter < 4)
            {
                if (damageRemaining <= 0f)
                    return;
                //var result = InteractionProbe.S.GetSimulation().Raycast(start, end);

                if (result.Succeeded && result.Collider != null)
                {

                    var rb = result.Collider as RigidbodyComponent;

                    if (penetrationLayer > 0)
                    {
                        var pentest = InteractionProbe.S.GetSimulation().Raycast(result.Point - 0.1f * forward.Normalized, start);
                        if (pentest.Succeeded)
                        {
                            var penDist = Vector3.Distance(pentest.Point, lastPenetrationPosition);
                            if (penDist > 2f)
                                return;
                        }
                    }

                   // if (damageRemaining < 0.8f && Vector3.dis)

                    if (rb != null && rb.AllowPush)
                    {
                        rb.Activate();
                        rb.ApplyImpulse(forward * Species.Knockback * damageRemaining);
                        rb.ApplyTorqueImpulse(forward * Species.Knockback * damageRemaining + new Vector3(0, 1, 0));
                    }

                    if (result.Collider.Entity.GetInterfaceInParent<IDamageable>() is IDamageable damageable)
                    {
                        if (damageable is SimDamageable sd && sd.Actor.State == Owner)
                            continue;
                        damageable.Damage(new DamageInfo
                        {
                            Faction = User.Faction,
                            Amount = Species.Damage,
                            Point = result.Point,
                            Knockback = Species.Knockback,
                            Forward = forward,
                            Perpetrator = User

                        });
                        if (damageable is ScriptComponent sc && sc.Entity.GetInterface<IPerceptible>() is IPerceptible ifaction)
                            EventManager.Raise(new ShootingIncident
                            {
                                Shooter = User,
                                GotShot = ifaction,
                            });

                        if (damageable.Penetrable || result.Collider.SurfaceType == SurfaceType.Flesh || result.Collider.SurfaceType == SurfaceType.Glass)
                        {
                            SurfaceEffectRegistry.S.BulletImpact(result, true, forward);
                            continue;
                        }
                        else
                        {

                            switch (result.Collider.SurfaceType)
                            {
                                case SurfaceType.Wood:
                                case SurfaceType.Snow:
                                    damageRemaining *= 0.66f;
                                    break;

                                default:
                                    damageRemaining *= 0.5f;
                                    break;

                            }

                            lastPenetrationPosition = result.Point;
                            penetrationLayer++;
                            SurfaceEffectRegistry.S.BulletImpact(result, false, forward);
                        }
                    }
                    else
                    {
                        if (result.Collider.SurfaceType == SurfaceType.Wood || result.Collider.SurfaceType == SurfaceType.Glass)
                        {
                            lastPenetrationPosition = result.Point;
                            penetrationLayer++;
                            SurfaceEffectRegistry.S.BulletImpact(result, false, forward);
                        }
                        else
                        {
                            SurfaceEffectRegistry.S.BulletImpact(result, false, forward);
                            return;
                        }
                    }

                    //SurfaceEffectRegistry.S.DoTracer(start, end, forward, real.WorldRotation);
                }
            }
        }
    }

    [Serializable]
    public abstract class FireManagerBase
    {
        public Weapon Weapon;
        public ActorState Owner;

        public IUsableUser User;
        public ActorSpecies Species => Weapon.Species;

        public abstract void OnFireDown();
        public abstract void OnFireUp();
        public abstract void OnFireFrame();
        public abstract void ResetWeapon();

        public abstract void OnUnequip();
    }

    [Serializable]
    public class FullAutoFire : FireManagerBase
    {
        float LastLaunchTime;
        bool IsShooting;
        public override void OnFireDown()
        {
            if (Time.time > LastLaunchTime + Weapon.Animator.FireResetManual
                && Weapon.CanShoot()
            && Weapon.GetAmmoManager().TryUseAmmo(true))
            {
                IsShooting = true;
                LastLaunchTime = Time.time;
                Weapon.GetProjectileManager().Launch(Weapon.GetMuzzleTransform(), User.GetShootPosition(), User.GetShootForward());
                User.OnFireStart();
                if (Weapon.GetAmmoManager().IsEmpty())
                {
                    User.OnShoot(true);

                    Weapon.Animator.FireDown(true);
                }
                else
                {
                    User.OnShoot(false);
                    Weapon.Animator.FireDown(false);
                }
            }
            else
            {
                IsShooting = false;
            }
        }

        public override void OnFireFrame()
        {
            if (Time.time > LastLaunchTime + Weapon.Animator.FireResetAuto
                && Weapon.CanShoot())
            {
                if (Weapon.GetAmmoManager().TryUseAmmo())
                {
                    IsShooting = true;
                    LastLaunchTime = Time.time;
                    Weapon.GetProjectileManager().Launch(Weapon.GetMuzzleTransform(), User.GetShootPosition(), User.GetShootForward());
                    if (Weapon.GetAmmoManager().IsEmpty())
                    {
                        User.OnShoot(true);

                        Weapon.Animator.FireDown(true);
                    }
                    else
                    {
                        User.OnShoot(false);
                        Weapon.Animator.FireDown(false);
                    }
                }
                else
                {
                    User.OnFireEnd();

                    IsShooting = false;
                }
            }
        }

        public override void OnFireUp()
        {
            if (IsShooting)
            {
                IsShooting = false;

                Weapon.Animator.FireUp();
            }
        }

        public override void OnUnequip()
        {
            IsShooting = false;
        }

        public override void ResetWeapon()
        {
            IsShooting = false;
        }
    }

    [Serializable]
    public abstract class AmmoManagerBase
    {
        public Weapon Weapon;
        public ActorState Owner;

        public IUsableUser User;
        public ActorSpecies Species => Weapon.Species;
        public abstract bool TryUseAmmo(bool isFireDown = false);

        public abstract void ResetWeapon();

        public abstract bool IsEmpty();
        public abstract bool TryReload(bool isFireDown = false);

        public virtual void OnUnequip()
        {

        }

        public virtual void OnBeginUnequip()
        {

        }
    }

    [Serializable]
    public class MagAmmoManager : AmmoManagerBase
    {

        int GetCurrentMag()
        {
            if (Weapon.Species.IsFists)
                return 99;
            if (Weapon.GetBound().Vars.TryGetValue("ammo", out var strval)
                && int.TryParse(strval, out var saved))
            {
                return saved;
            }
            else
            {
                return 0;
            }
        }
        public override bool IsEmpty()
        {
            return !Weapon.Species.IsFists &&
                GetCurrentMag() == 0;
        }

        public override void ResetWeapon()
        {
            isReloading = false;
        }

        public override void OnBeginUnequip()
        {
            base.OnBeginUnequip();
        }

        public override void OnUnequip()
        {
            base.OnUnequip();
            isReloading = false;
        }

        public override bool TryUseAmmo(bool isFireDown = false)
        {
            if (isReloading || !Weapon.GetIsReady())
                return false;

            if (Weapon.Species.IsFists)
                return true;

            var cMag = GetCurrentMag();
            if (cMag > 0)
            {
                Cvars.Set($"{Weapon.GetBound().SeqId}:ammo", (cMag - 1).ToString());
                return true;
            }
            else
            {
                TryReload(isFireDown);
                return false;
            }
        }

        public override bool TryReload(bool isFireDown = false)
        {
            if (isReloading || !Weapon.GetIsReady() || Weapon.Species.IsFists)
                return false;

            var cMag = GetCurrentMag();
            if (cMag < 0) cMag = 0;
            if (cMag == Species.ClipSize)
                return false;

            if (Owner.Vars.TryGetValue(Species.AmmoType, out var strval)
               && int.TryParse(strval, out var stashed))
            {
                if (stashed == 0)
                {
                    if (isFireDown)
                    {
                        User.OnReloadNoAmmo();
                        Weapon.Animator.ReloadNoAmmo();
                    }
                    return false;
                }

                if (cMag == 0)
                {
                    User.OnReloadStart(true);
                    Weapon.Animator.Reload(true);
                }
                else
                {
                    User.OnReloadStart(false);
                    Weapon.Animator.Reload(false);
                }


                var toUse = MathF.Min(stashed, Species.ClipSize - cMag);
                G.S.Script.AddTask(() => ReloadRoutine(async () =>
                {
                    Weapon.Animator.ReloadSuccess();
                    Cvars.Set($"{Owner.SeqId}:{Species.AmmoType}", (stashed - toUse).ToString());
                    Cvars.Set($"{Weapon.GetBound().SeqId}:ammo", $"{cMag + toUse}");
                }));

                return true;
            }
            else
            {
                return false;
            }
        }

        bool isReloading;

        public bool IsReloading => isReloading;

        async Task ReloadRoutine(Func<Task> doReload)
        {
            isReloading = true;
            await Task.Delay(Weapon.Animator.ReloadMs);

            isReloading = false;
            if (Weapon.GetIsReady())
            {
                User.OnReloadEnd();
                await doReload.Invoke();
            }
        }
    }
    public class InfiniteAmmoManager : AmmoManagerBase
    {
        public override bool IsEmpty()
        {
            return false;
        }

        public override void ResetWeapon()
        {
        }

        public override bool TryUseAmmo(bool isFireDown = false)
        {
            return true;
        }

        public override bool TryReload(bool asdf = false)
        {
            return false;
        }
    }

    public interface IUsableUser : IPerceptible
    {
        Entity ParentEnt { get; }
        void Bind(Weapon weapon);
        void OnReloadStart(bool isEmpty);
        void OnReloadEnd();
        void OnReloadNoAmmo();
        void OnUnequip();

        void OnShoot(bool holdOpen);
        void OnFireStart();
        void OnFireEnd();
        float GetSpread();

        Vector3 GetShootPosition();
        Vector3 GetShootForward();
        bool IsPlayer();
    }
}
