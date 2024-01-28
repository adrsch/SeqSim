//GPLv3 License

using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public struct CharacterMoveUpdate
    {
        public Vector3 WorldForward;
        public Vector3 Velocity;
        public bool IsGrounded;
        public bool IsRunning;
        public float Height;
        public bool Jumped;
    }

    public enum AnimState
    {
        none,
        stand,
        walk,
        dead,
        reloadwalk,
        reloadstand,
        shootstand,
        shootwalk,
        spotted,
        hit,
        flagged,
        flaggedfriend,
    }
    public class VariationInfo
    {
        public int VariationCount;
        public bool HasAlert;
        public string Alert;
        public bool HasHurt;
        public string Hurt;
        public bool HasBruised;
        public string Bruised;
        public bool HasPanic;
        public string Panic;
        public bool HasAlertHurt;
        public string AlertHurt;
        public bool HasAlertBruised;
        public string AlertBruised;
    }
    public class CharacterAnimator : StartupScript
    {
        public AnimationComponent Anims;
        public AudioEmitterComponent Emitter;
        public NavMeshAgent Agent;
        public AnimState State;
        public Vector3 lastPos;
        Weapon Weapon;
        Dictionary<AnimState, VariationInfo> Variations = new Dictionary<AnimState, VariationInfo>();
        public void SetWeapon(Weapon w) { Weapon = w; }
        public void Spotted()
        {
            Anims.PlayIfExists("spotted");
            Emitter.Oneshot("spotted");
            Overrided = true;
            State = AnimState.none;
        }
        DamageInfo lastDamaged;
        public FactionAI AI;
        public void Damaged(DamageInfo inf, bool stunned)
        {
            lastDamaged = inf;
            Anims.PlayIfExists("hit");
            Emitter.Oneshot("hit");
            if (!stunned)
                return;
            Overrided = true;
            State = AnimState.none;
        }

        bool Alert;

        public bool Overrided;

        public bool Dead;

        public bool Reloading => Weapon?.AmmoManager.IsReloading ?? false;


        public override void Start()
        {
            Anims ??= Entity.GetInChildren<AnimationComponent>();
            Anims.Play("stand");
            foreach (var s in Enum.GetValues(typeof(AnimState)))
            {
                var info = new VariationInfo();
                Variations[(AnimState)s] = info;
                var count = 0;
                if (Anims.Animations.ContainsKey(s.ToString()))
                    count++;
                while (Anims.Animations.ContainsKey($"{s}{count}"))
                {
                    count++;
                }
                info.VariationCount = count;

                info.Alert = $"{s}alert";
                info.HasAlert = Anims.Animations.ContainsKey(info.Alert);
                info.Bruised = $"{s}bruised";
                info.HasBruised = Anims.Animations.ContainsKey(info.Bruised);
                info.Hurt = $"{s}hurt";
                info.HasHurt = Anims.Animations.ContainsKey(info.Hurt);
                info.AlertBruised = $"{s}alertbruised";
                info.HasAlertBruised = Anims.Animations.ContainsKey(info.AlertBruised);
                info.AlertHurt = $"{s}alerthurt";
                info.HasAlertHurt = Anims.Animations.ContainsKey(info.AlertHurt);
                info.Panic = $"{s}panic";
                info.HasPanic = Anims.Animations.ContainsKey(info.Panic);
            }
        }

        public void SetAlert(bool alert)
        {
            // force refresh
            if (Alert != alert)
                State = AnimState.none;
            Alert = alert;
        }

        public void To(AnimState state)
        {
            BlendToState(state);
        }

        void BlendToState(AnimState state, int ms = 50)
        {
            if (State == state || state == AnimState.none || Variations[state].VariationCount == 0)
            {
                return;
            }
            var v = Random.Shared.Next(Variations[state].VariationCount + 1);

         //   Anims.PlayIfExistsAndNotPlaying(GetAnimName(state,v));
           // Anims.BlendIfExists(GetAnimName(state,v), 1f, TimeSpan.FromMilliseconds(ms));
            Anims.CrossfadeIfExists(GetAnimName(state, v), 1f, TimeSpan.FromMilliseconds(ms));
            State = state;
        }

        void BlendToAnim(string anim)
        {
            //   Anims.PlayIfExistsAndNotPlaying(anim);
            Anims.CrossfadeIfExists(anim, 1f, TimeSpan.FromMilliseconds(50));
            //Anims.BlendIfExists(anim, 1f, TimeSpan.FromMilliseconds(250));
        }

        string GetAnimName(AnimState state, int v) 
        {
            var info = Variations[state];
            if (info.HasHurt && AI.Status == PerceptibleStatus.Hurt)
            {
                if (Alert && info.HasAlertHurt)
                    return info.AlertHurt;
                return info.Hurt;
            }
            else if (info.HasBruised && AI.ShotBy != null)
            {
                if (Alert && info.HasAlertBruised)
                    return info.AlertBruised;
                return info.Bruised;
            }
            else if (info.HasAlert && Alert)
            {
                return info.Alert;
            }
            return v == 0 ? state.ToString() : $"{state}{v}";
        }

        Vector3 RealVelocity;
        public void Die()
        {
            var r = Random.Shared.Next(2);
            if (RealVelocity.Magnitude > 5f)
            {
                if (r < 1)
                {
                    Logger.Log(Channel.AI, LogPriority.Trace, $"Dying walking: {Entity.Name}");
                    BlendToAnim("deadwalk");
                }
                else
                {
                    Logger.Log(Channel.AI, LogPriority.Trace, $"Dying normal: {Entity.Name}");
                    BlendToAnim("dead");
                }
            }
            else
            {
                if (Vector3.Angle(lastDamaged.Forward, Transform.Forward) > 90)
                {
                    Logger.Log(Channel.AI, LogPriority.Trace, $"Dying normal: {Entity.Name}");
                    BlendToAnim("dead");
                }
                else
                {
                    if (r < 1)
                    {
                        Logger.Log(Channel.AI, LogPriority.Trace, $"Dying back: {Entity.Name}");
                    BlendToAnim("deadback");
                    }
                    else
                    {
                        Logger.Log(Channel.AI, LogPriority.Trace, $"Dying normal: {Entity.Name}");
                        BlendToAnim("dead");
                    }
                }
                /*
                var r = Random.Shared.Next(1);
                if (r == 0)
                {
                    BlendToAnim("dead");
                }
                else
                {
                    BlendToAnim("dead1");
                }*/
            }
            Agent.HaltMovement();
            State = AnimState.dead;
            //BlendToState(CharacterAnimationState.Dead);
            return;
        }

        [DataMemberIgnore]
        public bool Shooting => AI.FireDown;

        public void UpdateMovement()
        {
            RealVelocity = Agent.Transform.WorldPosition - lastPos;
            lastPos = Agent.Transform.WorldPosition;

            if (RealVelocity.Magnitude > 0)
            {
                if (Shooting)
                    BlendToState(AnimState.shootwalk, 30);
                else if (Reloading)
                    BlendToState(AnimState.reloadwalk);
                else 
                    BlendToState(AnimState.walk);
            }
            else
            {
                if (Shooting)
                    BlendToState(AnimState.shootstand, 30);
                else if (Reloading)
                    BlendToState(AnimState.reloadstand);
                else
                    BlendToState(AnimState.stand);
            }
            return;
        }
    }
}