using Silk.NET.OpenXR;
using Stride.Core;
using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public partial class FactionAI
    {
        bool Dead;
        public bool Death()
        {
            if (Damageable.IsDead)
            {
                if (!Dead)
                {
                    Agent.OrientToSurfaceNormal();
                    Actor.DisablePhysics();
                    ShotBy = null;
                    TImesShot = 0;
                    Animator.Die();
                    Dead = true;
                }
                return true;
            }
            else
            {
                if (Dead)
                {
                    Agent.OrientToWorldUp();
                    Actor.EnablPhysics();
                    ShotBy = null;
                    TImesShot = 0;
                    Animator.Dead = false;

                    foreach (var d in SensorAggregator.Detected.Values)
                        d.HighValueTarget = false;

                    Dead = false;
                }
                return false;
            }
        }


        bool Stunned;
        int WaitTime;
        public void Stun(AnimState state, int ms)
        {
            Animator.To(state);
            WaitTime = ms;
             Stunned = true;
            G.S.Script.AddTask(async () =>
            {
                await Task.Delay(WaitTime);
                Stunned = false;
                if (Animator.State == state)
                    Animator.State = AnimState.none;
            });
        }

        bool Reloading => CurrentWeapon?.AmmoManager.IsReloading ?? false;
        public void StartReload()
        {
            CurrentWeapon?.OnFireUp();
            CurrentWeapon?.Reload();
        }

        public bool ReloadThink()
        {
            var ammo = CurrentWeapon?.HasAmmo() ?? true;
            if (!ammo && !Reloading)
            {
                StartReload();
                return true;
            }
            else
            {
                return false;
            }
        }

        [DataMemberIgnore]
        public bool FireDown;
        public bool ShootThink()
        {
            var fists = CurrentWeapon?.Species.IsFists == true;
            /*
            if (FireDown && CurrentWeapon?.Species.IsFists == true)
            {
                var distance = Vector3.Distance(Transform.WorldPosition, EffectiveTarget.Position);
                if (distance > CurrentWeapon.Species.AIRangeMin)
                {
                    CancelShooting();
                    return false;
                }
                return true;
            
            }*/
            if (!fists && Goals.IsLineOfSightBlocked(this, EffectiveTarget))
            {
                CancelShooting();
                return false;
            }
            else if (Burst > ((fists) ? 1 : 2))
            {
                CanShootTime = Time.time + 1f;
                CancelShooting();
                return false;
            }
            else
            {
                if ( FireDown)
                {
                    CurrentWeapon?.OnFireDown();
                //    if (fists)
                  //      Agent.HaltMovement();
                    return true;
                }
                else
                {
                    return false;
                }
                var distance = Vector3.Distance(Transform.WorldPosition, EffectiveTarget.Position);
                var shouldbe = //Agent.IsFacing(EffectiveTarget.Position) && 
           CurrentWeapon != null
            && distance > CurrentWeapon.Species.AIRangeMin
            && distance < CurrentWeapon.Species.AIRangeMax;
                if (!shouldbe)
                    CancelShooting();

                return false;
            }
        }

        void CancelShooting()
        {
            FireDown = false;
            Burst = 0;
            CurrentWeapon?.OnFireUp();
        }

        public bool TryShooting(Vector3 p)
        {
            var distance = Vector3.Distance(Transform.WorldPosition, p);
            if (CanShootTime < Time.time
            && Agent.IsFacing(p)
            && CurrentWeapon != null
            && distance > CurrentWeapon.Species.AIRangeMin
            && distance < CurrentWeapon.Species.AIRangeMax)
            {
                FireDown = true;
                CurrentWeapon?.OnFireDown();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Chase(Vector3 position)
        {
            Goals.SetNavmeshForChasing(this);
            // Logger.Log(Channel.AI, LogPriority.Trace, $"{ai.DisplayName} entering chase");
            Agent.SetDestination(position);
            IsRunning = true;
        }

        public void Escape(Vector3 position, bool wasidling)
        {
           /* var d = (Transform.WorldPosition - position);
            if (d.Magnitude < 50)
            {
                Goals.ToPointAround(this, Transform.WorldPosition + d.Normalized * 4f);
                Idling = false;
            }
            else
            {*/
                ToIdle(wasidling);
                if (!wasidling)
                    Goals.Wander(this);
           // }
        }

        public void CantShoot(Vector3 p)
        {
            FireDown = false;
            CancelShooting();
            var distance = Vector3.Distance(Transform.WorldPosition, p);
            Agent.Navmesh = NavmeshType.Smash;
            if (distance <= MathF.Max(2, CurrentWeapon?.Species.AIRangeMin ?? 2))
            {
                Agent.SetDestination(
                    (Transform.WorldPosition - p).Normalized * 1.2f * CurrentWeapon.Species.AIRangeMin
                    + Transform.WorldPosition
                    , true);
            }
            else
            {
                if (!IsArmed|| distance > 30)
                    Agent.SetDestination(
                        (p - Transform.WorldPosition).Normalized * - 2f
                        + p
                        , true);
                // face the target
                else
                    Agent.SetDestination(Vector3.Lerp(p, Transform.WorldPosition, 0.95f), true);
            }
            IsRunning = true;
        }
        [DataMemberIgnore]
        public bool Idling;

        public async Task IdleAsync()
        {
            while (true)
            {
                if (Idling)
                {
                    Goals.Wander(this);
                }
                await Task.Delay(100);
            }
        }

        public void ToIdle(bool wasidling)
        {
            if (Idling && wasidling)
            {
                return;
            }
            else
            {
                if (!wasidling)
                {
                    if (Mood == MoodType.panic)
                        Goals.Wander(this);
                    else
                        Agent.HaltMovement();
                }
                else
                {

                }
                Idling = true;
            }
        }
    }
}
