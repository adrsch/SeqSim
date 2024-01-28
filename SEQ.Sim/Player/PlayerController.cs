using System;
using System.Diagnostics;
using BulletSharp;
using BulletSharp.SoftBody;
using Silk.NET.Core.Native;
using Silk.NET.OpenXR;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Shaders.Ast;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Lights;
using Int2 = Stride.Core.Mathematics.Int2;
using MathUtil = Stride.Core.Mathematics.MathUtil;
using SEQ.Script;
using SharpFont;
using SEQ.Sim;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    // TODO:
    // Move height control here
    // move update here
    // Make the derived class just do velocity updates
    // move feet here or get rid of the system
    public abstract class PlayerController : SyncScript, ICharacterMovement, IActorComponent
    {
        public static PlayerController S;
        [Display(category: "Refs")]
        public CharacterComponent Character;
        [Display(category: "Refs")]
        public FpsCamera FPSCam;
        [Display(category: "Refs")]
        public PlayerInput InputManager;

        [Display(category: "Events")]
        public EventWrapper LandedEvent = new EventWrapper();
        [Display(category: "Events")]
        public EventWrapper UngroundedEvent = new EventWrapper();
        [Display(category: "Events")]
        public EventWrapper JumpedEvent = new EventWrapper();

        public System.Action OnPush;

        [NonSerialized]
        public float currentHeight = 4.8f;
        [NonSerialized]
        public float currentRadius = 1.6f;

        [DataMemberIgnore]
        public bool IsGrounded;

        [DataMemberIgnore]
        public Actor PlayerActor;

        public override void Start()
        {
            S = this;
            //   Inspector.FindFreeInspector(Services).Target = this;
            base.Start();

            currentHeight = (GetDefaultHeight()).ToXenko();
            currentRadius = (GetDefaultRadius()).ToXenko();
        }

        public abstract float GetDefaultHeight();
        public abstract float GetDefaultRadius();
        public abstract void OnPhysicsUpdate(float dt);
        public abstract void OnReset();
        public virtual void OnCreation(Actor actor)
        {
            S = this;
            Character.SetCharacterMovement(this);
            PlayerActor = actor;
            actor.OnPositionChangedAction += HandlePositionChanged;
        }

        protected abstract void HandlePositionChanged(Vector3 pos, Quaternion rot);

        public abstract Vector3 Velocity { get; }
        public abstract Vector3 Position { get; }
        public abstract Vector3 ScaledVelocity { get; }
    }
}
