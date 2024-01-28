using BulletSharp;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class PlayerFootstepPlayer : SyncScript
    {
        [Stride.Core.DataContract]
        public class SurfaceEffectInfo
        {
            public SurfaceType SurfaceType;
            public string StepSfx;
            public string LandSfx;
        }
        public List<SurfaceEffectInfo> Effects = new List<SurfaceEffectInfo>();
        public AudioEmitterComponent AudioEmitter;
        public PlayerController PlayerMovement;
        float FootstepTimeSprint = 0.34f;
        float FootstepTimeWalk = 0.44f;
        float GroundResetTime = 0.3f;
        float VelocityCutoff = 1f;
        float SprintVelocity = 170f;

        public override void Start()
        {
            base.Start();
            PlayerMovement.JumpedEvent.Event += () => Jump();
        }

        float LastPlayedTime;
        bool WasGrounded;
        public override void Update()
        {
        //    if (GroundNextFrame)
          //  {
            //    Land();
              //  return;
            //}
            var velocity = new Vector2(PlayerMovement.Velocity.x, PlayerMovement.Velocity.z).Length();

            var moving = velocity > VelocityCutoff.ToXenko();
            if ( !moving && PlayerMovement.GroundCollider is RigidbodyComponent Rb && Rb.AngularVelocity.Length() > 0.5f)
            {
                moving = true; 
            }

            var footstepTime = velocity > SprintVelocity.ToXenko() ? FootstepTimeSprint : FootstepTimeWalk;

            if (PlayerMovement.IsGrounded)
            {
                if (Time.time > footstepTime + LastPlayedTime && moving)
                {
                    Play(false);
                }
                else if (!WasGrounded && Time.time > GroundResetTime + LastPlayedTime)
                {
                    Play(false);
                }
                else if (!moving)
                {
                    LastPlayedTime = 0;
                }

            }
            WasGrounded = PlayerMovement.IsGrounded;

        }

      //  bool GroundNextFrame = false;

        void Jump()
        {
            var velocity = new Vector2(PlayerMovement.Velocity.x, PlayerMovement.Velocity.z).Length();

            var footstepTime = velocity > SprintVelocity.ToXenko() ? FootstepTimeSprint : FootstepTimeWalk;

            if (Time.time > GroundResetTime + LastPlayedTime)
            {
                Play(true);
            }
            //GroundNextFrame = false;
            WasGrounded = false;
        }

        public void Play(bool landed)
        {
            //	type = OverrideType != SurfaceType.Default ? OverrideType : type;
            foreach (var c in Effects)
            {
                if (c.SurfaceType == PlayerMovement.GroundSurface)
                {
                    LastPlayedTime = Time.time;
                    AudioEmitter.Oneshot(landed ? c.LandSfx : c.StepSfx);


                    foreach (var prefect in SurfaceEffectRegistry.S.Footsteps)
                    {

                        if (prefect.Surface == c.SurfaceType)
                        {
                            var ent = prefect.Prefab.InstantiateTemporary(SurfaceEffectRegistry.S.Entity.Scene, prefect.Lifetime);
                            ent.Transform.WorldPosition = PlayerMovement.Position - Vector3.up * PlayerMovement.currentHeight * 0.58f;
                            //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
                            var footdir = PlayerCamera.S.Transform.Forward;
                            footdir.y = 0;
                            ent.Transform.Rotation = Quaternion.LookRotation( in PlayerMovement.GroundHitNormal, in footdir) ;

                            return;
                        }
                    }
                    return;
                }
            }
        }
    }
}
