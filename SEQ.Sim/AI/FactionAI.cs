
using Silk.NET.OpenXR;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum MoveSpeedClass
    {
        Dead,
        Walk,
        Run,
        Shoot,
        Reload,
    }

    public enum OrderType
    {
        panic,
        hunt,
        follow,
        stand,
    }

    public enum MoodType
    {
        calm,
        determined,
        panic,
        frenzy,
    }
    public partial class FactionAI : AsyncScript, IPerceptible, IUsableUser, ISolveSpeed
    {
      //  public string DisplayName => Entity.Name;
        public Actor Actor;
        public NavMeshAgent Agent;
        public SimDamageable Damageable;
        public CharacterAnimator Animator;
        //  public EventWrapper SpottedEvent;
        [DataMemberIgnore]
        public SensorAggregator SensorAggregator;
        public ISensorTarget LastTarget { get; private set; }
        public ISensorTarget Target { get; private set; }
        public ISensorTarget EffectiveTarget => Target ?? LastTarget;
        public PerceptibleStatus Status => Damageable.IsDead ? PerceptibleStatus.Dead : Damageable.IsDamaged ? PerceptibleStatus.Hurt : PerceptibleStatus.Default;
        public IFactionProvder Faction { get; set; }

        public TransformComponent ShootTransform;

        [Display(category: "Movement", order: 10)]
        public float WalkSpeed = 75f;
        [Display(category: "Movement", order: 10)]
        public float RunSpeed = 115f;
        [Display(category: "Movement", order: 10)]
        public float ShootSpeed = 10f;
        [Display(category: "Movement", order: 10)]
        public float ReloadSpeed = 30f;

        public bool IsRunning;
        public MoveSpeedClass SpeedClass;
        
        public float GetSpeed()
        {

            if (CurrentWeapon != null && CurrentWeapon.AmmoManager.IsReloading)
                return ReloadSpeed;
            else if (FireDown)
                return IsArmed ? ShootSpeed : 0f;
            else if (IsRunning || Mood == MoodType.panic)
                return Status == PerceptibleStatus.Hurt ? RunSpeed * 0.5f : RunSpeed;
            else
                return Status == PerceptibleStatus.Hurt ? WalkSpeed * 0.5f : WalkSpeed;
        }
        public AudioEmitter AudioPerceptible { get; set; }
        [DataMemberIgnore]
        public List<VisualEmitter> VisualPerceptibles { get; set; } = new List<VisualEmitter>();

        [DataMemberIgnore]
        public string WeaponSpecies;

        void GetSpeciesWeapon()
        {
            var st = ActorSpeciesRegistry.SpawnNewState(WeaponSpecies);
            Actor.State.AddChild(st.SeqId);
            CurrentWeapon = ActorUsable.Get(st, this) as Weapon;
            CurrentWeapon.IsReady = true;
            CurrentWeapon.ProjectileManager.OnShotsFired += OnShotsFired;
            Animator.SetWeapon(CurrentWeapon);
        }

        [DataMemberIgnore]
        public IPerceptible ShotBy;

        public AIRandomizer Randomizer;

        [DataMemberIgnore]
        public HumanInteractable Interactable;

        public Vector3 GetPosition() => Transform.WorldPosition;
        void ActorChagned()
        {
            if (Actor.State.Vars.ContainsKey("faction"))
            {
                var stringFaction = Actor.State.GetVar<string>("faction");
                foreach (var rel in FactionManager.S.Relations)
                {
                    if (rel.Cvar == stringFaction)
                    {
                        Faction = rel.Provider;
                    }
                }
            }

            if (Actor.State.Vars.ContainsKey("weapon"))
                WeaponSpecies = Actor.State.GetVar<string>("weapon");
            if (Randomizer != null)
            {

            Randomizer.Set(Randomizer.HeadIndex, Randomizer.HeadVariants, Actor.State.GetVar<int>("headseed"));
            Randomizer.Set(Randomizer.UpperIndex, Randomizer.UpperVariants, Actor.State.GetVar<int>("upperseed"));
            Randomizer.Set(Randomizer.LowerIndex, Randomizer.LowerVariants, Actor.State.GetVar<int>("lowerseed"));
            Randomizer.Set(Randomizer.ShoeIndex, Randomizer.ShoeVariants, Actor.State.GetVar<int>("shoeseed"));

            }
            if (Interactable != null && Actor.State.Vars.ContainsKey("name"))
            {
                Interactable.Name = Actor.State.Vars["name"];
            }
        }

        public override async Task Execute()
        {
            Interactable = Entity.Get<HumanInteractable>();
            Agent.SpeedSolver = this;
            SensorAggregator = new SensorAggregator(Entity.GetInterfacesInChildren<ISensor>().ToList());
            SensorAggregator.This = this;
            Actor.ActiveUpdate += DoUpdate;
            Animator.lastPos = Agent.Transform.WorldPosition;
            Damageable.DieAction += () => Die();
            Damageable.DamagedAction += OnDamaged;
            Animator.AI = this;


            if (Randomizer != null && !Actor.State.Vars.ContainsKey("headseed"))
            {
                Actor.State.Vars["headseed"] = Random.Shared.Next(Randomizer.HeadVariants.Count).ToString();
                Actor.State.Vars["upperseed"] = Random.Shared.Next(Randomizer.UpperVariants.Count).ToString();
                Actor.State.Vars["lowerseed"] = Random.Shared.Next(Randomizer.LowerVariants.Count).ToString();
                Actor.State.Vars["shoeseed"] = Random.Shared.Next(Randomizer.ShoeVariants.Count).ToString();
            }

            Actor.OnValueChanged += ActorChagned;
            ActorChagned();


            GeneratePersonality();

            if (!string.IsNullOrWhiteSpace(WeaponSpecies))
            {
                GetSpeciesWeapon();
            }

            World.RegisterPerceptible(this);

            G.S.Script.AddTask(IdleAsync);

            while (true)
            {
                SensorAggregator.UpdateDetections();
                Evaluate();
                await Task.Delay(100);
            }
            /*
            if (!Damageable.IsDead)
            {
                Actor.EnablPhysics();
                Animator.Dead = false;
                SensorAggregator.UpdateDetections();

                if (FireDown)
                {
                    if (!HasAmmo())
                    {
                        Goals.StopShoot(this);
                        Goals.Reload(this);
                    }
                    else if (Goals.IsLineOfSightBlocked(this, EffectiveTarget))
                    {
                        Goals.StopShoot(this);
                    }
                    else if (Burst > 2)
                    {
                        Goals.StopShoot(this);
                        CanShootTime = Time.time + 1f;
                    }
                    else
                    {
                        Goals.Shoot(this);
                    }
                }
                else
                {
                    if (!HasAmmo())
                    {
                        Goals.Reload(this);
                    }
                    else if (TrySpot())
                    {
                        //  SpottedEvent.Invoke(Transform);

                        Animator.SetAlert(true);
                        if (!Stunned)
                        {
                            Animator.Spotted();
                            await Task.Delay(666);
                            if (!Stunned)
                                Animator.Overrided = false;
                        }
                    }
                    else if (LastTarget != null && Goals.IsHostileOrUnsure(this, LastTarget))
                    {
                        Animator.SetAlert(true);
                        if (Target == null || Goals.IsLineOfSightBlocked(this, Target))
                        {
                            Goals.Chase(this, SpottedPosition);
                        }
                        else if (Goals.IsHostile(this, EffectiveTarget))
                        {
                            Goals.TryToShoot(this, LastTarget.Position);
                        }
                        else
                        {
                            Goals.Chase(this, SpottedPosition);
                            // TODO
                            //   Activities.TryToExamine(this, LastTarget.Position);
                        }
                    }
                    else
                    {
                        Animator.SetAlert(false);
                        Goals.Wander(this);
                    }
                }

            }
            else
            {
                Actor.DisablePhysics();
                Animator.Dead = true;
            }
            await Task.Delay(100);
            while (Stunned)
                await Task.Delay(50);

        }*/
        }

        void Evaluate()
        {
            UpdateOrderFromCvar();
            var wasidling = Idling;
            Idling = false;
            var lastMood = Mood;
            if (!Death()
                && !Stunned
                && !ReloadThink()
                && !ShootThink())
            {
                if (TrySpot())
                {
                    EvaluateMood(true);
                    //  SpottedEvent.Invoke(Transform);

                    Animator.SetAlert(true);
                    Stun(AnimState.spotted, 666);
                }
                else if (LastTarget != null && Goals.IsHostileOrUnsure(this, LastTarget))
                {
                    EvaluateMood(true);

                    if (Mood == MoodType.panic && lastMood != MoodType.panic)
                    {
                        FlaggedStunTime = Time.time;
                        Stun(AnimState.flagged, 2500);
                    }
                    else
                    {

                        Animator.SetAlert(Mood != MoodType.calm);

                        if (Mood == MoodType.frenzy ||
                            (Mood == MoodType.determined && Order == OrderType.hunt))
                        {
                            if (Target == null || Goals.IsLineOfSightBlocked(this, Target))
                            {
                                Chase(SpottedPosition);
                            }
                            else if (Goals.IsHostile(this, EffectiveTarget))
                            {
                                if (!TryShooting(LastTarget.Position))
                                {
                                    CantShoot(LastTarget.Position);
                                }
                            }
                            else
                            {
                                Chase(SpottedPosition);
                                // TODO
                                //   Activities.TryToExamine(this, LastTarget.Position);
                            }
                        }
                        else if ((Mood == MoodType.determined || Mood == MoodType.calm))
                        {
                            if (Target == null || Goals.IsLineOfSightBlocked(this, Target))
                            {
                                ToIdle(wasidling);
                                //   Chase(SpottedPosition);
                            }
                            else if (Goals.IsHostile(this, EffectiveTarget))
                            {
                                if (Order != OrderType.panic || Status != PerceptibleStatus.Hurt)
                                {
                                    if (!TryShooting(LastTarget.Position))
                                    {
                                        CantShoot(LastTarget.Position);
                                    }
                                }
                                else
                                {
                                    Escape(LastTarget.Position, wasidling);
                                }
                            }
                            else
                            {
                                ToIdle(wasidling);
                                //   Chase(SpottedPosition);
                                // TODO
                                //   Activities.TryToExamine(this, LastTarget.Position);
                            }
                        }
                        else
                        {
                            Escape(LastTarget.Position, wasidling);
                        }
                    }
                }
                else
                {
                    EvaluateMood(false);
                    Animator.SetAlert(false);
                    ToIdle(wasidling);
                }
            }
        }

        [DataMemberIgnore]
        public float CanShootTime;

        public OrderType Order = OrderType.panic;
        void UpdateOrderFromCvar()
        {
            Order = Actor.State.GetVar<OrderType>("order");
        }
        public IPerceptible Commander;
        public MoodType Mood;

        public int Burst;
       // [DataMemberIgnore]
       // public bool FireDown;
        void OnShotsFired()
        {
            Burst++;
        }

        public Entity ParentEnt => Entity;

        public bool IsArmed => CurrentWeapon != null && !CurrentWeapon.Species.IsFists;
        public bool Moving;
        private void DoUpdate(float dt)
        {
            if (!Dead && !Stunned)
            {
                Agent.UpdateMovement(dt, this);
                Animator.UpdateMovement();
            }
        }

        float FlaggedStunTime;
        public void OnFlagged()
        {
            if (!Dead && !IsArmed && !Stunned && !FireDown && FlaggedStunTime + 2f < Time.time)
            {
                FlaggedStunTime = Time.time;
                if ((Fortitude > 0.99f) || (
                    (Mood == MoodType.determined || Mood == MoodType.calm) &&
                    Fortitude > 0.5f
                    ))
                    Stun(AnimState.flaggedfriend, 666);
                else
                    Stun(AnimState.flagged, 2500);

            }
        }

        int TImesShot;
        void OnDamaged(DamageInfo inf)
        {
            TImesShot++;
            if (SensorAggregator.Detected.ContainsKey(inf.Perpetrator))
                ShotBy = inf.Perpetrator;
            if (FireDown)
            {
                Animator.Damaged(inf, false);
                return;
            }

            Animator.Damaged(inf, true);

            //Stunned = true;

            TrySpot(); // no stun after
            if (!Stunned)
            {
                Stun(AnimState.hit, 666);
            }

            /*
            G.ScriptSystem.AddTask(async () =>
            {
                await Task.Delay(666);
                Stunned = false;
                Animator.Overrided = false;
            });
            */
        }

        public void Die()
        {
            Evaluate();
        }

        float lastSpottedTime;
        public float SpottedCooldown = 2;

        [DataMemberIgnore]
        public Vector3 SpottedPosition;
        [DataMemberIgnore]
        public  ISensor SpottingSensor;
        bool TrySpot()
        {
            var newt = SensorAggregator.Target;
            var isSpotted = newt != null && Target == null && lastSpottedTime + SpottedCooldown < Time.time;

            if (newt != null)
            {
                Target = newt;
                lastSpottedTime = Time.time;
                LastTarget = Target;
                SpottingSensor = SensorAggregator.SpottingSensor;
                SpottedPosition = Target.NavPosition();
            }
            else if (lastSpottedTime + SpottedCooldown > Time.time)
            {
                Target = LastTarget;
            }
            else
            {
                SpottingSensor = null;
                Target = null;
            }

            return isSpotted;
        }



        public void DoScare()
        {

            //if (!Damageable.IsDead)
            //    Activity.Scare();
        }

        [DataMemberIgnore]
        public Weapon CurrentWeapon;
        public void Bind(Weapon weapon)
        {
            CurrentWeapon = weapon;
        }

        public void OnReloadStart(bool isEmpty)
        {
            //Animator.Reloading = true;
        }

        public void OnReloadEnd()
        {
            //Animator.Reloading = false;
        }

        public void OnReloadNoAmmo()
        {
        }

        public void OnUnequip()
        {
        }

        public void OnShoot(bool holdOpen)
        {
        }

        public void OnFireStart()
        {
            //Animator.Shooting = true;
        }

        public void OnFireEnd()
        {
            // Animator.Shooting = false;
            Burst = 0;
        }

        public float GetSpread()
        {
            return 0.1f;
        }

        public Vector3 GetShootPosition()
        {
            return SensorAggregator.SpottingSensor != null 
                ? (SensorAggregator.SpottingSensor.SensorPosition(true))
                : ShootTransform.WorldPosition;
        }

        public Vector3 GetShootForward()
        {
            if (Target != null)
                return Target.Position - GetShootPosition();
            else if (LastTarget  != null)
                return LastTarget.Position - GetShootPosition();
            return ShootTransform.Forward;
        }

        public bool IsPlayer()
        {
            return false;
        }
    }
}