// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
/*
 * PARTS OF THIS FILE ARE MODIFICATIONS OF QUAKE 3 CODE UNDER THE FOLLOWING:
===========================================================================
Copyright (C) 1999-2005 Id Software, Inc.

This file is part of Quake III Arena source code.

Quake III Arena source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

Quake III Arena source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
Find the GPL at the bottom.
*/
using System;
using System.Diagnostics;
using BulletSharp;
using BulletSharp.SoftBody;
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
using SEQ.Sim;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class QController : PlayerController
    {
        [DataMemberIgnore]
        public Entity HookEntity;
        [DataMemberIgnore]
        public bool HookActive;

        [Display(category: "Physics")]
        public float GroundedDistance = 16;
        bool IsStable;
        [DataMemberIgnore]
        public bool IsSlick;
        [DataMemberIgnore]
        public bool IsSwimming;
        [DataMemberIgnore]
        public bool InWater;
        [DataMemberIgnore]
        public bool IsKnockback;
        [DataMemberIgnore]
        public bool IsOnRb;

        [Display(category: "Physics")]
        public float MaxGroundAngle;
        [DataMemberIgnore]
        public Vector3 LocalMove;
        [DataMemberIgnore]
        public Vector3 GroundHitNormal;

        [DataMemberIgnore]
        public override Vector3 Velocity => CurrentVelocity.ToXenko();
        [DataMemberIgnore]
        public override Vector3 ScaledVelocity => CurrentVelocity;
        [DataMemberIgnore]
        public override Vector3 Position => CurrentPosition.ToXenko();



        bool JumpQueued;
        float LastJumpedTime;
        const float MinJumpTime = 0.1f;
        const float SameSurfaceThreshold = 0.99f;

        /*    public Vector3 CharacterPosition
            {
                get => Character?.GetPhysicsPosition() ?? Vector3.zero;
                set => Character?.SetPhysicsPosition(value);
            }*/

        protected override void HandlePositionChanged(Vector3 pos, Quaternion rot)
        {
            CurrentPosition = pos;
            CurrentVelocity = PlayerActor.State.Velocity;
            Character.SetPhysicsPosition(pos);
            LastPosition = pos;
            LastLastPosition = pos;
            LastLastLastPosition = pos;
        }

        // BAD: Should have proper input support for frame updates
        bool DidCrouchDown;
        public override void Update()
        {
            DidCrouchDown = Keybinds.GetKeyDown(Keybind.Crouch);
            // BAD: lazy move out of physics update
            DoDuck();
            //  Character.ColliderShape.Margin
            DebugText.Print($"{((int)CurrentVelocity.Length())}", new Int2(100, 280));
            DebugText.Print($"{((int)(new Vector3(CurrentVelocity.x, 0, CurrentVelocity.z)).Length())}", new Int2(100, 300));
            DebugText.Print($"Grounded: {IsGrounded} Stable: {IsStable} On Rb: {IsOnRb} WaterLevel: {WaterLevel} Knockback {IsKnockback}", new Int2(100, 320));
            DebugText.Print($"Velocity: {(int)CurrentVelocity.x}, {(int)CurrentVelocity.y}, {(int)CurrentVelocity.z}", new Int2(100, 340));
            DebugText.Print($"Position: {(int)Entity.Transform.WorldPosition.x}, {(int)Entity.Transform.WorldPosition.y}, {(int)Entity.Transform.WorldPosition.z}", new Int2(100, 360));
          //  DebugText.Print($"Physics Position: {Character.GetPhysicsPosition}", new Int2(100, 380));


            if (IsMovementEnabled && Game.Window.Focused)
            {
                {
                    var module = G.S.GetModule<ITouchModule>();
                    if (module == null ||!module.Initialized || (module.Initialized && !module.TouchEnabled))
                    {
                        Input.LockMousePosition(true);
                    }
                    Game.IsMouseVisible = false;
                }
            }
            else
                Input.UnlockMousePosition();


            if (PhysicsConstants.Autobhop)
            {
                JumpQueued = IsMovementEnabled && (
                    Keybinds.GetKey(Keybind.Jump)
                    || Input.IsGamePadButtonDownAny(Stride.Input.GamePadButton.A)
                    || Input.IsGamePadButtonDownAny(Stride.Input.GamePadButton.LeftShoulder)
                    );
            }
            else
            {
                if (IsMovementEnabled)
                {
                    if (Keybinds.GetKeyDown(Keybind.Jump))
                    {
                        JumpQueued = true;
                    }
                    else if (Keybinds.GetKeyUp(Keybind.Jump))
                    {
                        JumpQueued = false;
                    }
                    if (Input.IsGamePadButtonDownAny(Stride.Input.GamePadButton.A)
                    || Input.IsGamePadButtonDownAny(Stride.Input.GamePadButton.LeftShoulder))
                    {
                        if (hasLetUpJumpButton)
                            JumpQueued = true;
                    }
                    else
                    {
                        hasLetUpJumpButton = true;
                    }
                }
                else
                {
                    JumpQueued = false;
                    hasLetUpJumpButton = true;
                }
            }
        }
        public bool GetIsGrounded()
        {
            return IsGrounded;
        }
        BulletSharp.CharacterSweepCallback DoSweep(Vector3 from, Vector3 to, float? addedMargin = null)
        {
           // Character.AddedMargin = addedMargin ?? BaseAddedMargin;
            var sweep = Character.DoSweep(from.ToXenko(), to.ToXenko());
           // Character.AddedMargin = BaseAddedMargin;
            sweep.Point = sweep.Point.FromXenko();
            if (sweep.HitCollisionObject?.UserObject is RigidbodyComponent rb)
            {
                if (!rb.IsKinematic && rb.AllowPush)
                {
                    rb.Activate(true);
                }
                // hack
                else if (rb.SurfaceType == SurfaceType.Glass && rb.Entity.GetInParent<GlassProp>() is GlassProp gp)
                {
                    if (gp.CanWalkThrough)
                    {
                        SurfaceEffectRegistry.S.ForceEffect(new HitResult
                        {
                            Collider = rb,
                            Normal = sweep.Normal,
                            Point = sweep.Point.ToXenko(),
                            HitFraction = sweep.HitFraction,
                            Succeeded = true,
                        });
                        gp.DestroySegment(rb.Entity);
                    }
                }
            }
            return sweep;
        }

        public PhysicsComponent GroundCollider;
        public SurfaceType GroundSurface => GroundCollider?.SurfaceType ?? SurfaceType.Default;
        Vector3 surfaceVelocity;

        public Vector3 GroundHitPoint;
        void GroundSweep(float dt)
        {
            // TODO
            IsSlick = false;
            var pos = CurrentPosition;
            var from = Character.PhysicsWorldTransform;
            from.TranslationVector = pos;
            //var transposed = Character.PhysicsWorldTransform;
            //transposed.TranslationVector = pos - GroundedDistance * Vector3.UnitY;
            //  var sweep = Character.GhostSweep(from, transposed, +GroundedDistance * Vector3.UnitY);
            SetSkin(0, -Skin);
            var sweep = DoSweep(pos + Skin * Vector3.UnitY, pos - (GroundedDistance + Skin) * Vector3.UnitY);
            SetSkin(0, 0);
            //var sweep = Character.Simulation.ShapeSweep(Character.ColliderShape, from, transposed);
            GroundHitNormal = sweep.Normal;
            GroundHitPoint = sweep.Point;
            if (!sweep.Succeeded)
            {
                Freefall();
                LeftGroundCheck();
                IsGrounded = false;
                IsStable = false;
                IsOnRb = false;
              //  CurrentVelocity += surfaceVelocity * 0.5f;
                surfaceVelocity = Vector3.zero;
                lastAirTime = Time.time;
                return;
            }
            else if (GroundHitNormal.y < 0.2f)
            {
                LeftGroundCheck();
                IsGrounded = false;
                IsStable = false;
                IsOnRb = false;
                lastAirTime = Time.time;
                return;
            }
            GroundCollider = sweep.HitCollisionObject?.UserObject as PhysicsComponent;
            IsSlick = GroundSurface == SurfaceType.Ice;
            if (GroundCollider is RigidbodyComponent rb)
            {
                IsOnRb = true;
                surfaceVelocity = rb.LinearVelocity.FromXenko();
                CurrentPosition = CurrentPosition + surfaceVelocity * dt;

                Character.SetPhysicsPosition(CurrentPosition.ToXenko());

                Character.ApplyPosition(CurrentPosition.ToXenko(), true, true);

            }
            else
            {
                IsOnRb = false;
             //   CurrentVelocity += surfaceVelocity * 0.5f;
                surfaceVelocity = Vector3.zero;
             //   if (GroundCollider != lastGroundCollider)
              // {
                //  CurrentVelocity += surfaceVelocity * 0.5f;
             // }
            }
            // idk what this is tbh
            if (CurrentVelocity.y > 0 && Vector3.Dot(CurrentVelocity, GroundHitNormal) > 10)
            {
                LeftGroundCheck();
                IsGrounded = false;
                IsStable = false;
                lastAirTime = Time.time;
                return;
            }
            if (GroundHitNormal.y < 0.7f)
            {
                LeftGroundCheck();
                IsGrounded = true;
                IsStable = false;
                lastAirTime = Time.time;
                return;
            }
            if (!IsGrounded)
            {
                LandedEvent?.Invoke(Transform);
                EventManager.Raise(new LandedEvent());
            }
            IsGrounded = true;
            IsStable = true;
        }
        float lastAirTime;
        bool CanStand()
        {
            var pos = CurrentPosition;
            var from = Character.PhysicsWorldTransform;
            from.TranslationVector = pos;
         //   var transposed = Character.PhysicsWorldTransform;
            var transposed = pos + height * Vector3.UnitY;
            //  var sweep = Character.GhostSweep(from, transposed, +GroundedDistance * Vector3.UnitY);
            var sweep = DoSweep(pos, transposed);
            //var sweep = Character.Simulation.ShapeSweep(Character.ColliderShape, from, transposed);
            return !sweep.Succeeded;
        }

        void Freefall()
        {
            /*
            if (Time.time + LastJumpedTime >  Time.time || IsGrounded)
            {
                var from = Character.PhysicsWorldTransform;
                from.TranslationVector = CurrentPosition;
                var transposed = Character.PhysicsWorldTransform;
                transposed.TranslationVector = CurrentPosition - 64 * Vector3.UnitY;
                var sweep = Character.GhostSweep(from, transposed);
                if (!sweep.Succeeded)
                {

                }
            }*/
        }

        void LeftGroundCheck()
        {
            //if (IsCrouched)
            //    Uncrouch();
            if (IsGrounded)
                UngroundedEvent?.Invoke(Transform);
        }

        bool IsCrouched;
        // BAD: hardcoded...
        const float height = 48f;
        const float radius = 16f;
        public override float GetDefaultHeight()
        {
            return height;
        }
        public override float GetDefaultRadius()
        {
            return radius;
        }

        bool QueueUncrouch = false;
        void DoDuck()
        {
            if (IsCrouched && DidCrouchDown)
            {
                QueueUncrouch = true;
              //  Uncrouch();
            }
            else if (!IsCrouched && DidCrouchDown)
            {
                Crouch();
            }
            // BAD: this is hack
            DidCrouchDown = false;
        }

        float currentHeightSkin = 0;
        float currentRadiusSkin = 0;
        void SetSkin(float heightSkin, float radiusSkin)
        {
            if (heightSkin == currentHeightSkin && radiusSkin == currentRadiusSkin)
                return;

            Character.ColliderShape = new CylinderColliderShape(
                currentHeight + heightSkin.ToXenko(),
                currentRadius + radiusSkin.ToXenko(),
                ShapeOrientation.UpY);
            Character.UpdateShape();
        }

        void Crouch()
        {
            //Character.ColliderShape.Scaling = new Vector3(1, 0.5f, 1);
         Character.ColliderShape = new CylinderColliderShape(
                2.4f,
                radius.ToXenko(),
                ShapeOrientation.UpY);
            //   Character.ColliderShape = new SphereColliderShape( false,
            // radius.ToXenko());
            //   {
            //     LocalOffset = new Vector3(0, 1.2f, 0)
            //   };

            currentHeight = 2.4f;
            currentRadius = radius.ToXenko();

        Character.UpdateShape();
            Character.SetPhysicsPosition(Character.GetPhysicsPosition().FromXenko() + new BulletSharp.Math.Vector3(0, 1, 0)  * (height / 2).ToXenko());
            Character.UpdatePhysicsTransformation(true);
            FPSCam.Entity.Transform.Position.y = 1f; // 1f = (2.4 / 2) - 0.2f
            IsCrouched = true;
            Feet.Position = new Vector3(0, (height).ToXenko() * -0.5f, 0);
        }
        void Uncrouch()
        {
            if (!CanStand())
                return;
            //  Character.ColliderShape.Scaling = new Vector3(1, 1, 1);

            Character.ColliderShape = new CylinderColliderShape(
                (height).ToXenko(),
                radius.ToXenko(),
                ShapeOrientation.UpY);
            //   {
            //    LocalOffset = new Vector3(Character.GetPhysicsPosition().X, Character.GetPhysicsPosition().Y + 2.4f, Character.GetPhysicsPosition().Z)
            // };
            currentHeight = (height).ToXenko();
            currentRadius = (radius).ToXenko();
            Character.UpdateShape();
            CurrentPosition += Vector3.up * (height).ToXenko();
            ApplyPosition(CurrentPosition, 0);
            //Character.ApplyPosition()
            // Character.SetPhysicsPosition(Character.GetPhysicsPosition().FromXenko() + new BulletSharp.Math.Vector3(0, 1, 0) * (height).ToXenko());

            Character.UpdatePhysicsTransformation(true);
            FPSCam.Entity.Transform.Position.y = 2.2f; // 4.6 - 2.4
            Feet.Position = new Vector3(0, (height).ToXenko() * - 0.5f, 0);
            IsCrouched = false;
        }

        public TransformComponent Feet;

        public override void OnReset()
        {

        }

        void Unstick()
        {

        }
        [DataMemberIgnore]
        public Vector3 LastPosition;
        [DataMemberIgnore]
        public Vector3 LastLastPosition;
        [DataMemberIgnore]
        public Vector3 LastLastLastPosition;

        // is this used?
        public event System.Action AfterPhysicsUpdate;

        bool hasLetUpJumpButton;
        //bool wasWarping;
        //Vector3 LastVelocity;
        public override void OnPhysicsUpdate(float deltaTime)
        {
           Character.SetUseGhost(true);
            // Character.ColliderShape.Margin = 0f;
            HitSpeed = 0;
            CurrentVelocity = Character.GetPhysicsVelocity().FromXenko();//Units.ToQuake(velocity);
                                       //  CurrentVelocity = velocity;//Units.ToQuake(velocity);
            CurrentPosition = Character.GetPhysicsPosition().FromXenko();//character.PhysicsWorldTransform.TranslationVector;

            if (/*!wasWarping &&*/ LastPosition == CurrentPosition && CurrentVelocity.Magnitude > 10)
            {
                CurrentPosition = LastLastLastPosition;
                ApplyPosition(CurrentPosition, deltaTime);
                Logger.Log(Channel.Physics, LogPriority.Warning, $"Last resort stuck detected!");
            }

           // wasWarping = false;
            //LastVelocity = CurrentVelocity;
            LastLastLastPosition = LastLastPosition;
            LastLastPosition = LastPosition;
            LastPosition = CurrentPosition;
            LocalMove = IsMovementEnabled ? InputManager.GetMoveDir() : Vector3.zero;

            if (PhysicsConstants.Noclip)
            {
                var wishdir = Utils.LogicDirectionToWorldDirection(new Vector2(LocalMove.X, LocalMove.Z), PlayerCamera.S.Camera, FPSCam.Transform.Up);

                CurrentVelocity = GetCurrentMaxSpeed() * wishdir;
                CurrentPosition = CurrentPosition + CurrentVelocity * deltaTime;

                Character.SetPhysicsPosition(CurrentPosition.ToXenko());
                Character.SetPhysicsVelocity(CurrentVelocity.ToXenko());  
                return;
            }
            GroundSweep(deltaTime);

            if (QueueUncrouch)
            {
                // total hack but it feels okay
                if (IsGrounded)
                    Jump(PhysicsConstants.JumpForce * 0.5f);
                else
                    Uncrouch();
                QueueUncrouch = false;
            }


            //untested
            if (HookActive)
            {
                var hookBindPos = (FPSCam.Transform.GetWorldPosition().FromXenko() - 0.2f * Vector3.UnitY);
                var wishdir = HookEntity.Transform.GetWorldPosition().FromXenko() - hookBindPos;


                //    if (Vector3.Angle(new Vector3(wishdir.x, 0, wishdir.z), wishdir) > 45f)
                IsGrounded = false;
                IsStable = false;



                var speed = HookFriction(deltaTime);
                CurrentVelocity += wishdir.Normalized * speed;
            }

            //untested
            if (WaterLevel > 1)
            {
                var wishdir = Utils.LogicDirectionToWorldDirection(new Vector2(LocalMove.X, LocalMove.Z), PlayerCamera.S.Camera, Vector3.UnitY);
                if (!HookActive)
                    CurrentVelocity = 150f * TransformToViewDirection(wishdir);//inputHandler.FPCamera.transform.TransformDirection(wishdir);

                //     currentVelocity = Units.ToUnity(150f) * transform.TransformDirection(wishdir);//inputHandler.FPCamera.transform.TransformDirection(wishdir);

                /*  var wave = Mathf.Sin(5f * Time.time);
                  if (transform.position.y <= -10.3)
                      currentVelocity.y = 1f;
                  else if (wave < 0)
                      currentVelocity.y = wave;*/
            }
            else if (IsStable)
            {

                GroundMove(deltaTime);
            }
            else
            {
                AirMove(deltaTime, false);
            }

            Character.ApplyPosition(CurrentPosition.ToXenko(), false, false);
            CurrentVelocity = Character.GetPhysicsVelocity().FromXenko();
            CurrentPosition = Character.GetPhysicsPosition().FromXenko();

            // this doesnt happen much anymore
            if (/*!wasWarping && */LastPosition == CurrentPosition && CurrentVelocity.Magnitude > 10)
            {
                CurrentPosition = LastPosition;
                Character.ApplyPosition(CurrentPosition.ToXenko(), false, false);
                Logger.Log(Channel.Physics, LogPriority.Trace, $"Stuck detected!");

                // hack for slopes
                CurrentPosition.y += StepHeight * 0.025f;
                IsStable = false;
                IsSlick = true;
               GroundMove(deltaTime);
                Character.ApplyPosition(CurrentPosition.ToXenko(), false, false);
                CurrentVelocity = Character.GetPhysicsVelocity().FromXenko();
                CurrentPosition = Character.GetPhysicsPosition().FromXenko();
            }

            AfterPhysicsUpdate?.Invoke();
        }

        /* this doesnt happen at all anymore
        void WarpCheck(float dt)
        {
            if (Vector3.DistanceSquared(CurrentPosition, LastPosition) > 1200 * dt * 1200)
            {
                CurrentPosition = LastPosition;
                //don't do this, if the move fails trying the new velocity on old position can fix
                //CurrentVelocity = LastVelocity;
                Character.ApplyPosition(CurrentPosition.ToXenko(), false, false);
                Logger.Log(Channel.Physics, LogPriority.Info, $"Warp detected!");
                wasWarping = true;
            }

        }
        */
        public bool IsMovementEnabled => GameStateManager.Inst.Active.GetInteractionState() == InteractionState.World;
        // Handle air movement.

        private void AirMove(float dt, bool jumped)
        {
            ApplyFriction(dt);

            var forward = FPSCam.Transform.Forward;
            var right = FPSCam.Transform.Right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            var wishdir = forward * LocalMove.z + right * LocalMove.x;
            wishdir.y = 0;
            var wishspeed = wishdir.Magnitude;
            wishspeed *= (jumped ? PhysicsConstants.JumpForce : PhysicsConstants.AirSpeed);

            wishdir.Normalize();

            float accel = PhysicsConstants.AirAccel;
            // this is from some unity engine quake thing i was using literally 3+ years ago

            // CPM Air control.
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(CurrentVelocity, wishdir) < 0)
            {
                accel = PhysicsConstants.AirStopSpeed;
            }

            // If the player is ONLY strafing left or right
           /* if (IsMovementEnabled && (LocalMove.Z == 0 && LocalMove.X != 0))
            {
                //     if (wishspeed > Settings.Strafe.MaxSpeed && UseMaxStrafeSpeed)
                //    {
                //       wishspeed = Settings.Strafe.MaxSpeed;
                //  }

                accel = PhysicsConstants.StrafeAccel;
            }
           */
            Accelerate(wishdir, wishspeed, accel, dt);

            if (PhysicsConstants.AirControl > 0)
            {
                AirControl(wishdir, wishspeed2, dt);
            }

            if (IsGrounded)
                CurrentVelocity = ProjectToNormal(CurrentVelocity, GroundHitNormal);
            StepSlideMove(true, dt);
        }

        // Air control occurs when the player is in the air, it allows players to move side 
        // to side much faster rather than being 'sluggish' when it comes to cornering.
        private void AirControl(Vector3 targetDir, float targetSpeed, float dt)
        {
            // Only control air movement when moving forward or backward.
            if (!IsMovementEnabled || (MathF.Abs(LocalMove.Z) < 0.001 || MathF.Abs(targetSpeed) < 0.001))
            {
                return;
            }

            float zSpeed = CurrentVelocity.Y;
            CurrentVelocity.Y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            float speed = CurrentVelocity.Length();
            CurrentVelocity.Normalize();

            //m_AirControl = 0f;
            float dot = Vector3.Dot(CurrentVelocity, targetDir);
            float k = 32;
            k *= PhysicsConstants.AirControl * dot * dot * dt;

            // Change direction while slowing down.
            if (dot > 0)
            {
                CurrentVelocity.X = CurrentVelocity.X * speed + targetDir.X * k;
                CurrentVelocity.Y = CurrentVelocity.Y * speed + targetDir.Y * k;
                CurrentVelocity.Z = CurrentVelocity.Z * speed + targetDir.Z * k;

                CurrentVelocity.Normalize();
                //m_MoveDirectionNorm = m_CharacterVelocity;
            }

            CurrentVelocity.X *= speed;
            CurrentVelocity.Y = zSpeed; // Note this line
            CurrentVelocity.Z *= speed;
        }
        void Jump(float force)
        {
            if (IsCrouched)
                Uncrouch();
            IsGrounded = false;
            IsStable = false;
            IsOnRb = false;
            LastJumpedTime = Time.time;
            JumpedEvent?.Invoke(Transform);
            EventManager.Raise(new JumpedEvent());
            CurrentVelocity.Y = force;
            if (!PhysicsConstants.Autobhop)
                JumpQueued = false;
        }

        Vector3 TransformToViewDirection(Vector3 input)
        {
            return Utils.LogicDirectionToWorldDirection(input, PlayerCamera.S.Camera, Vector3.UnitY);
        }

        float GetCurrentMaxSpeed()
        {
            if (PhysicsConstants.Noclip)
            {

                if (Keybinds.GetKey(Keybind.Sprint))
                    return PhysicsConstants.NoclipSprint;
                if (IsCrouched)
                    return PhysicsConstants.NoclipCrouch;
                return PhysicsConstants.NoclipSpeed;
            }
            float speed;
            if (Keybinds.GetKey(Keybind.Sprint))
                // BAD: should add crouchsprint state
                speed = IsCrouched ? 0.5f * PhysicsConstants.CrouchSpeed : PhysicsConstants.SprintSpeed;
            else if (IsCrouched)
                speed = PhysicsConstants.CrouchSpeed;
            else
                speed = PhysicsConstants.Speed;

            if (PhysicsConstants.AntiNoobhopTime > 0)
            {
                var antiNoobhop = MathUtil.Clamp01((Time.time - lastAirTime) / 0.5f);
                return antiNoobhop * antiNoobhop * speed;
            }
            else
            {
                return speed;
            }
        }

        float GetGroundAccel()
        {
            if (IsCrouched)
                return PhysicsConstants.CrouchAccel;
            if (Keybinds.GetKey(Keybind.Sprint))
                return PhysicsConstants.SprintAccel;
            return PhysicsConstants.Accel;
        }
        float GetCurrentStopSpeed()
        {
            if (IsCrouched)
                return PhysicsConstants.CrouchStopSpeed;
            if (Keybinds.GetKey(Keybind.Sprint))
                return PhysicsConstants.SprintStopSpeed;
            return PhysicsConstants.StopSpeed;
        }

        public float WaterLevel;

        void WaterMove(float dt)
        {

        }

        const int YAW = 1;
        const int PITCH = 0;
        const int ROLL = 2;
        [Display(category: "Physics")]
        public float Skin = 0f;//0.001f;
        [Display(category: "Physics")]
        public float Margin = 0.25f;
        [Display(category: "Physics")]
        public float BaseAddedMargin = 0.02f;
        void AngleVectors(Vector3 angles, out Vector3 forward, out Vector3 right, out Vector3 up)
        {
            float angle;
            float sr, sp, sy, cr, cp, cy;

            angle = angles[YAW] * MathUtil.Deg2Rad;
            sy = MathF.Sin(angle);
            cy = MathF.Cos(angle);
            angle = angles[PITCH] * MathUtil.Deg2Rad;
            sp = MathF.Sin(angle);
            cp = MathF.Cos(angle);
            angle = angles[ROLL] * MathUtil.Deg2Rad;
            sr = MathF.Sin(angle);
            cr = MathF.Cos(angle);

            forward = new Vector3(
                cp * cy,
                cp * sy,
                -sp);

            right = new Vector3(
                (-1 * sr * sp * cy + -1 * cr * -sy),
                (-1 * sr * sp * sy + -1 * cr * cy),
                -1 * sr * cp);

            up = new Vector3(
                (cr * sp * cy + -sr * -sy),
                (cr * sp * sy + -sr * cy),
                cr * cp);
        }

        
        private void GroundMove(float dt)
        {
            //  if (WaterLevel > 2 && DotProduct(pml.forward, pml.groundTrace.plane.normal) > 0)
            if (WaterLevel > 2 && FPSCam.Transform.Forward.Dot(GroundHitNormal) > 0)
            {
                // begin swimming
                WaterMove(dt);
                return;
            }
            if (JumpQueued && Time.time > LastJumpedTime + MinJumpTime)
            {
                if (WaterLevel > 1)
                    WaterMove(dt);
                else
                {
                    Jump(PhysicsConstants.JumpForce);
                    AirMove(dt, true);
                }
                return;
            }

            ApplyFriction(dt);

            var forward = FPSCam.Transform.Forward;
            var right = FPSCam.Transform.Right;

            forward.y = 0;
            right.y = 0;

            forward = ProjectToNormal(forward, GroundHitNormal).Normalized;
            right = ProjectToNormal(right, GroundHitNormal).Normalized;

            var wishdir = forward * LocalMove.z + right * LocalMove.x;

            var wishspeed = wishdir.Magnitude;
            wishspeed *= GetCurrentMaxSpeed();

            if (WaterLevel > 0)
            {
                var waterScale = WaterLevel / 3f;
                waterScale = 1 - (0.5f) * waterScale;
                if (wishspeed > GetCurrentMaxSpeed() * waterScale)
                {
                    wishspeed = wishspeed * GetCurrentMaxSpeed() * waterScale;
                }
            }

            var accel = GetGroundAccel();
            if (CurrentVelocity.Dot(wishdir) < 0)
                accel = GetCurrentStopSpeed();

            // hack for very forceful stopping
            if (InputManager.IsCancelling())
            {
                accel = GetCurrentStopSpeed();
                accel *= 2;
                if (CurrentVelocity.Magnitude < 150f)
                    accel *= 5;
            }

            if (IsSlick || IsKnockback)
                accel = PhysicsConstants.AirAccel;

            Accelerate(wishdir, wishspeed, accel, dt);


            if (!IsStable || IsKnockback)
            {

                CurrentVelocity.Y -= PhysicsConstants.Gravity * dt;
            }

            //var vel = CurrentVelocity.Length();
            //          var vel = CurrentVelocity.Length();
            //       CurrentVelocity = Clip(CurrentVelocity, GroundHitNormal);
            //    CurrentVelocity.Normalize();
            //  CurrentVelocity *= vel;

            // TODO: this looks iffy
          /*  if (CurrentVelocity.x == 0 && CurrentVelocity.z == 0)
            {
                CurrentVelocity = Vector3.zero;
                return;
            }*/

           StepSlideMove(false, dt);

        }
        [Display(category: "Physics")]
        public float StopMovingThreshold = 10f;
        [Display(category: "Physics")]
        public float StepHeight = 0.5f;
        void StepSlideMove(bool gravity, float dt)
        {
            if (!gravity && PlayerAnimator.S != null)
            {
                CurrentVelocity += Pushaway.Process(PlayerAnimator.S).FromXenko();
            }

            var pos = CurrentPosition;
            var vel = CurrentVelocity;
            if (!TryMove(gravity, dt)) return;

            var from = Character.PhysicsWorldTransform;
            from.TranslationVector = pos;

            var down = Character.PhysicsWorldTransform;
            var downTarget = pos - Vector3.up * StepHeight;
            down.TranslationVector = downTarget;
            //var sweep = Character.GhostSweep(from, down, pos - downTarget);
            var sweep = DoSweep(pos, downTarget);
            //  var sweep = Character.Simulation.ShapeSweep(Character.ColliderShape, from, down);
            var hitFraction = sweep.HitFraction;
            if (CurrentVelocity.y > 0 && (hitFraction >= 1f || sweep.Normal.Dot(Vector3.up) < 0.7f))
            {
                return;
            }

            var preStepPos = CurrentPosition;
            var preStepVel = CurrentVelocity;

            from = Character.PhysicsWorldTransform;
            from.TranslationVector = pos;
            var up = Character.PhysicsWorldTransform;
            var upTarget = pos + Vector3.up * StepHeight;
            up.TranslationVector = upTarget;
            sweep = DoSweep(pos, upTarget, BaseAddedMargin);

            hitFraction = sweep.HitFraction;

            var hitPoint = Vector3.Lerp(pos, upTarget, hitFraction);

            var dist = hitPoint.y - pos.y;
            ApplyPosition(hitPoint, dt);
            // CurrentPosition = sweep.Point;
            CurrentVelocity = vel;

            TryMove(gravity, dt);

            from = Character.PhysicsWorldTransform;
            from.TranslationVector = CurrentPosition;
            var correct = Character.PhysicsWorldTransform;
            var correctTarget = CurrentPosition - Vector3.up * dist;

            correct.TranslationVector = correctTarget;
            sweep = DoSweep(CurrentPosition, correctTarget, BaseAddedMargin);
            hitFraction = sweep.HitFraction;
            hitPoint = Vector3.Lerp(CurrentPosition, correctTarget, hitFraction);

            if (sweep.Succeeded)
            {
                CurrentVelocity = ProjectToNormal(CurrentVelocity, sweep.Normal);
                if (sweep.Normal.Y < 0.7f)
                {
                    CurrentVelocity = preStepVel;
                    CurrentPosition = preStepPos;
                    ApplyPosition(preStepPos, dt);
                }
                else
                {
                    ApplyPosition(hitPoint, dt);
                }
            }
            else
            {
                ApplyPosition(hitPoint, dt);
            }
        }

        const int maxSteps = 4;
        Vector3 CurrentPosition;
        float HitSpeed;

        Vector3 VectorMA(Vector3 va, float scale, Vector3 vb)
        {
            return new Vector3(va.x + scale * vb.x, va.y + scale * vb.y, va.z + scale * vb.z);
        }

        void ApplyPosition(Vector3 position, float dt)
        {
            CurrentPosition = position;
            Character.SetPhysicsPosition(CurrentPosition.ToXenko());
            Character.SetPhysicsVelocity(CurrentVelocity.ToXenko());
            Character.ApplyPosition(CurrentPosition, true, true);
        }

        [DataMemberIgnore]
        public Vector3 CurrentVelocity;

        bool TryMove(bool gravity, float dt)
        {
            var pv = CurrentVelocity;
            var endVelocity = Vector3.zero;

            if (gravity)
            {
                endVelocity = CurrentVelocity;
                endVelocity.y -= PhysicsConstants.Gravity * dt;
                CurrentVelocity.y = 0.5f * (CurrentVelocity.y + endVelocity.y);
                pv.y = endVelocity.y;
                if (IsGrounded)
                {
                    ProjectToNormal(CurrentVelocity, GroundHitNormal);
                }
            }

            Vector3[] surfaces = new Vector3[maxSteps + 1];
            RigidbodyComponent[] rbs = new RigidbodyComponent[maxSteps + 1];
            var surfaceCount = 0;
            if (IsGrounded)
            {
                surfaces[0] = GroundHitNormal;
                surfaceCount = 1;
            }

            surfaces[surfaceCount] = CurrentVelocity.Normalized;
            surfaceCount++;

            
            var remaining = dt;
            var step = 0;
            for (step = 0; step < maxSteps; step++)
            {
                // var targetPosition = CurrentPosition + remaining * CurrentVelocity;
                var targetPosition = VectorMA(CurrentPosition, remaining, CurrentVelocity);
                var from = Character.PhysicsWorldTransform;
                from.TranslationVector = CurrentPosition;
                var to = Character.PhysicsWorldTransform;
                to.TranslationVector = targetPosition;
                var sweep = DoSweep(CurrentPosition, targetPosition);
                var hitFraction = sweep.HitFraction;
                // if (hitFraction < float.Epsilon)
                // {
                //     CurrentVelocity.y = 0;
                //     return true;
                // }
                var positionBeforeApply = CurrentPosition;
                // if (hitFraction > 0)
                    // ApplyPosition(Vector3.Lerp(CurrentPosition, targetPosition, hitFraction - Skin), remaining);

                if (!sweep.Succeeded)
                {
                    ApplyPosition(targetPosition, dt);
                    break;
                }

                if (sweep.HitCollisionObject?.UserObject is RigidbodyComponent rb && !rb.IsKinematic && rb.AllowPush)
                {
                    var applyForce = true;
                    foreach (var a in rbs) { if (a == rb) { applyForce = false; break; } }
                    if (applyForce)
                    {
                        rb.Activate(true);
                        rb.ApplyImpulse(CurrentVelocity.ToXenko() * 0.05f / rb.Mass);
                        OnPush?.Invoke();
                        rbs[step] = rb;
                    }
                }

                //since we aren't actually moving if there's any hit, remaining will always be the full amount 
                //remaining -= remaining * hitFraction;
                if (surfaceCount >= maxSteps)
                {
                    break;
                    // CurrentVelocity = Vector3.zero;
                    // Logger.Log(Channel.Gameplay, LogPriority.Error, "Couldn't solve move");
                    //return true;
                }
                var i = 0;
                for (i = 0; i < surfaceCount; i++)
                {
                    if (sweep.Normal.Dot(surfaces[i]) > 0.99f)
                    {
                        CurrentVelocity += new Vector3(sweep.Normal.X, sweep.Normal.Y, sweep.Normal.Z);
                        break;
                    }
                }
                if (i < surfaceCount) continue;

                surfaces[surfaceCount] = sweep.Normal;
                surfaceCount++;

                for (i = 0; i < surfaceCount; i++)
                {
                    var into = CurrentVelocity.Dot(surfaces[i]);
                    if (into >= 0.1)
                        continue;
                    if (-into > HitSpeed)
                    {
                        HitSpeed = -into;
                    }

                    var clipVel = ProjectToNormal(CurrentVelocity, surfaces[i]);
                    var clipEnd = ProjectToNormal(endVelocity, surfaces[i]);

                    for (var j = 0; j < surfaceCount; j++)
                    {
                        if (j == i) continue;
                        if (clipVel.Dot(surfaces[j]) >= 0.1) continue;

                        clipVel = ProjectToNormal(clipVel, surfaces[j]);
                        clipEnd = ProjectToNormal(clipEnd, surfaces[j]);

                        if (clipVel.Dot(surfaces[i]) >= 0)
                            continue;

                        var dir = Vector3.Cross(surfaces[i], surfaces[j]).Normalized;
                        var d = dir.Dot(CurrentVelocity);
                        clipVel = dir * d;
                        d = dir.Dot(endVelocity);
                        clipEnd = dir * d;

                        for (var k = 0; k < surfaceCount; k++)
                        {
                            if (k == i || k == j) continue;
                            if (clipVel.Dot(surfaces[k]) >= 0.1) continue;

                            CurrentVelocity = Vector3.zero;
                            return true;
                        }
                    }

                    CurrentVelocity = clipVel;
                    endVelocity = clipEnd;
                    break;
                }
            }
                
            if (gravity)
                CurrentVelocity = endVelocity;

            Character.SetPhysicsPosition(CurrentPosition.ToXenko());
            Character.SetPhysicsVelocity(CurrentVelocity.ToXenko());
            return step != 0;
        }

        Vector3 ProjectToNormal(Vector3 u, Vector3 v)
        {
            var backoff = Vector3.Dot(u, v);
            if (backoff < 0)
            {
                backoff *= 1.001f;
            }
            else
            {
                backoff /= 1.001f;
            }

            var change = v * backoff;
            return new Vector3(
                   u.x - change.x,
                   u.y - change.y,
                   u.z - change.z);
        }

        private void ApplyFriction(float dt)
        {
            var vec = CurrentVelocity;

            if (IsStable)
                vec.y = 0;

            float speed = vec.Length();
            if (speed < 1)
            {
                CurrentVelocity.x = 0;
                CurrentVelocity.z = 0;
                return;
            }

            float drop = 0;

            if (WaterLevel <= 1
                && IsStable
                && !IsSlick
                && !IsKnockback)
            {
                float control = ((float)(speed) < GetCurrentStopSpeed()) ? ((float)GetCurrentStopSpeed()) : speed;
                drop = control * PhysicsConstants.Friction * dt;
            }

            if (WaterLevel > 0)
                drop += speed * WaterLevel * dt;

            float newSpeed = speed - drop;
            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            newSpeed /= speed;

            CurrentVelocity.X *= newSpeed;
            CurrentVelocity.Y *= newSpeed;
            CurrentVelocity.Z *= newSpeed;
        }

        //untested
        private float HookFriction(float dt)
        {
            Vector3 vec = CurrentVelocity;
            vec.Y = 0;
            //  float speed = Units.ToUnity(800f);
            float speed = 1200f;
            float drop = 0;

            // Only apply friction when grounded.
            if (GetIsGrounded() && Vector3.Distance(Transform.GetWorldPosition().FromXenko(), HookEntity.Transform.GetWorldPosition().FromXenko()) < 8f)
            {
                var amount = 1f - Stride.Core.Mathematics.MathUtil.Clamp01((Vector3.Distance(Transform.GetWorldPosition(), HookEntity.Transform.GetWorldPosition()).FromXenko() - 3f) / 5f);
                //float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
                drop = 15f * PhysicsConstants.Friction * dt * amount;
            }
            float newSpeed = speed - drop;
            //m_PlayerFriction = newSpeed;
            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (speed > 0)
            {
                newSpeed /= speed;
            }
            CurrentVelocity.X *= newSpeed;
            CurrentVelocity.Z *= newSpeed;


            return (10f + PhysicsConstants.Gravity * dt) * Stride.Core.Mathematics.MathUtil.Clamp01(Vector3.Distance(Transform.GetWorldPosition(), HookEntity.Transform.GetWorldPosition()).FromXenko() / 5.0f);


        }

        // This isn't used right now
        bool UseBhopData;
        public BhopData BhopData = new BhopData();
        public Vector3 FeetWorld => Transform.GetWorldPosition() - Vector3.up * currentHeight * 0.5f;
        private void Accelerate(Vector3 wishdir, float wishspeed, float accel, float dt)
        {
            var beforeAccel = CurrentVelocity;
            var currentspeed = Vector3.Dot(CurrentVelocity, wishdir);

            float addspeed = wishspeed - currentspeed;

            if (UseBhopData)
            {
                if (BhopData.AddSpeed == 0)
                {
                    BhopData.Angle = CurrentVelocity.Angle(wishdir);
                    BhopData.LastVelocity = CurrentVelocity.Length();
                    BhopData.AccelSpeed = accel * dt * wishspeed;
                    BhopData.TargetSpeed = wishspeed;
                }
                BhopData.DidUpdate = true;
            }

            if (addspeed <= 0)
            {
                return;
            }

            float accelspeed = accel * dt * wishspeed;
            if (accelspeed > wishspeed) accelspeed = addspeed;

            CurrentVelocity.X += accelspeed * wishdir.X;
            CurrentVelocity.Y += accelspeed * wishdir.Y;
            CurrentVelocity.Z += accelspeed * wishdir.Z;

            if (UseBhopData)
            {
                if (accelspeed > BhopData.AddSpeed)
                {
                    BhopData.Angle = CurrentVelocity.Angle(wishdir);
                    BhopData.AddSpeed = addspeed;
                    BhopData.AccelSpeed = accelspeed;
                    BhopData.LastVelocity = CurrentVelocity.Length();
                    BhopData.TargetSpeed = wishspeed;
                }
            }

            // Special handling for walking ontop of rigidbodies
            if (IsGrounded && GroundCollider is RigidbodyComponent rb
                && CurrentVelocity.Magnitude > beforeAccel.Magnitude)
            {
                if (rb.IsSphere())
                {
                    var delta = CurrentVelocity - beforeAccel;
                    CurrentVelocity = beforeAccel;
                    rb.LinearVelocity += delta.ToXenko();
                }
                else if (rb.IsCylinder(out var axis))
                {
                    var delta = CurrentVelocity - beforeAccel;
                    var flat = new Vector3(delta.x, 0, delta.z);
                    // CurrentVelocity = beforeAccel;
                    var ang = Vector3.Angle(axis, flat);
                    // 
                    //     rb.LinearVelocity += flat.ToXenko();
                    /*
                    if (ang < 150 && ang > 60 && Vector3.Angle(axis, Vector3.up) > 45)
                    {

                        rb.Activate(true);
                        var flatClipped = flat.Normalized * MathF.Min(flat.Magnitude * 0.5f, 50f);
                        rb.ApplyImpulse(flatClipped.ToXenko(), FeetWorld - rb.Transform.WorldPosition);
                        if (flat.Magnitude * 0.5f < 50f)
                        {
                            CurrentVelocity = Vector3.Lerp(
                                CurrentVelocity,
                                new Vector3(beforeAccel.x, CurrentVelocity.y, beforeAccel.z),
                                0.5f);
                        }
                        else
                        {
                            CurrentVelocity = CurrentVelocity.Normalized * (CurrentVelocity.Magnitude - 50f);
                        }
                    }
                    else
                    {*/
                        rb.Activate(true);
                        var flatClipped = flat.Normalized * MathF.Min(flat.Magnitude * 0.5f, 50f);
                        rb.ApplyImpulse(flatClipped.ToXenko(), FeetWorld - rb.Transform.WorldPosition);
                    //}
               //     rb.AngularVelocity += (axis.Cross(CurrentVelocity).y < 0 ? 1 : -1)
                 //     * axis.Normalized * delta.Magnitude.ToXenko() * 0.3f;

                }
            }
        }
    }
}

/*
 * THE QUAKE 3 CODE IS LICENSED UNDER THE FOLLOWING:
 * 		    GNU GENERAL PUBLIC LICENSE
		       Version 2, June 1991

 Copyright (C) 1989, 1991 Free Software Foundation, Inc.
                       51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

			    Preamble

  The licenses for most software are designed to take away your
freedom to share and change it.  By contrast, the GNU General Public
License is intended to guarantee your freedom to share and change free
software--to make sure the software is free for all its users.  This
General Public License applies to most of the Free Software
Foundation's software and to any other program whose authors commit to
using it.  (Some other Free Software Foundation software is covered by
the GNU Library General Public License instead.)  You can apply it to
your programs, too.

  When we speak of free software, we are referring to freedom, not
price.  Our General Public Licenses are designed to make sure that you
have the freedom to distribute copies of free software (and charge for
this service if you wish), that you receive source code or can get it
if you want it, that you can change the software or use pieces of it
in new free programs; and that you know you can do these things.

  To protect your rights, we need to make restrictions that forbid
anyone to deny you these rights or to ask you to surrender the rights.
These restrictions translate to certain responsibilities for you if you
distribute copies of the software, or if you modify it.

  For example, if you distribute copies of such a program, whether
gratis or for a fee, you must give the recipients all the rights that
you have.  You must make sure that they, too, receive or can get the
source code.  And you must show them these terms so they know their
rights.

  We protect your rights with two steps: (1) copyright the software, and
(2) offer you this license which gives you legal permission to copy,
distribute and/or modify the software.

  Also, for each author's protection and ours, we want to make certain
that everyone understands that there is no warranty for this free
software.  If the software is modified by someone else and passed on, we
want its recipients to know that what they have is not the original, so
that any problems introduced by others will not reflect on the original
authors' reputations.

  Finally, any free program is threatened constantly by software
patents.  We wish to avoid the danger that redistributors of a free
program will individually obtain patent licenses, in effect making the
program proprietary.  To prevent this, we have made it clear that any
patent must be licensed for everyone's free use or not licensed at all.

  The precise terms and conditions for copying, distribution and
modification follow.

		    GNU GENERAL PUBLIC LICENSE
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION

  0. This License applies to any program or other work which contains
a notice placed by the copyright holder saying it may be distributed
under the terms of this General Public License.  The "Program", below,
refers to any such program or work, and a "work based on the Program"
means either the Program or any derivative work under copyright law:
that is to say, a work containing the Program or a portion of it,
either verbatim or with modifications and/or translated into another
language.  (Hereinafter, translation is included without limitation in
the term "modification".)  Each licensee is addressed as "you".

Activities other than copying, distribution and modification are not
covered by this License; they are outside its scope.  The act of
running the Program is not restricted, and the output from the Program
is covered only if its contents constitute a work based on the
Program (independent of having been made by running the Program).
Whether that is true depends on what the Program does.

  1. You may copy and distribute verbatim copies of the Program's
source code as you receive it, in any medium, provided that you
conspicuously and appropriately publish on each copy an appropriate
copyright notice and disclaimer of warranty; keep intact all the
notices that refer to this License and to the absence of any warranty;
and give any other recipients of the Program a copy of this License
along with the Program.

You may charge a fee for the physical act of transferring a copy, and
you may at your option offer warranty protection in exchange for a fee.

  2. You may modify your copy or copies of the Program or any portion
of it, thus forming a work based on the Program, and copy and
distribute such modifications or work under the terms of Section 1
above, provided that you also meet all of these conditions:

    a) You must cause the modified files to carry prominent notices
    stating that you changed the files and the date of any change.

    b) You must cause any work that you distribute or publish, that in
    whole or in part contains or is derived from the Program or any
    part thereof, to be licensed as a whole at no charge to all third
    parties under the terms of this License.

    c) If the modified program normally reads commands interactively
    when run, you must cause it, when started running for such
    interactive use in the most ordinary way, to print or display an
    announcement including an appropriate copyright notice and a
    notice that there is no warranty (or else, saying that you provide
    a warranty) and that users may redistribute the program under
    these conditions, and telling the user how to view a copy of this
    License.  (Exception: if the Program itself is interactive but
    does not normally print such an announcement, your work based on
    the Program is not required to print an announcement.)

These requirements apply to the modified work as a whole.  If
identifiable sections of that work are not derived from the Program,
and can be reasonably considered independent and separate works in
themselves, then this License, and its terms, do not apply to those
sections when you distribute them as separate works.  But when you
distribute the same sections as part of a whole which is a work based
on the Program, the distribution of the whole must be on the terms of
this License, whose permissions for other licensees extend to the
entire whole, and thus to each and every part regardless of who wrote it.

Thus, it is not the intent of this section to claim rights or contest
your rights to work written entirely by you; rather, the intent is to
exercise the right to control the distribution of derivative or
collective works based on the Program.

In addition, mere aggregation of another work not based on the Program
with the Program (or with a work based on the Program) on a volume of
a storage or distribution medium does not bring the other work under
the scope of this License.

  3. You may copy and distribute the Program (or a work based on it,
under Section 2) in object code or executable form under the terms of
Sections 1 and 2 above provided that you also do one of the following:

    a) Accompany it with the complete corresponding machine-readable
    source code, which must be distributed under the terms of Sections
    1 and 2 above on a medium customarily used for software interchange; or,

    b) Accompany it with a written offer, valid for at least three
    years, to give any third party, for a charge no more than your
    cost of physically performing source distribution, a complete
    machine-readable copy of the corresponding source code, to be
    distributed under the terms of Sections 1 and 2 above on a medium
    customarily used for software interchange; or,

    c) Accompany it with the information you received as to the offer
    to distribute corresponding source code.  (This alternative is
    allowed only for noncommercial distribution and only if you
    received the program in object code or executable form with such
    an offer, in accord with Subsection b above.)

The source code for a work means the preferred form of the work for
making modifications to it.  For an executable work, complete source
code means all the source code for all modules it contains, plus any
associated interface definition files, plus the scripts used to
control compilation and installation of the executable.  However, as a
special exception, the source code distributed need not include
anything that is normally distributed (in either source or binary
form) with the major components (compiler, kernel, and so on) of the
operating system on which the executable runs, unless that component
itself accompanies the executable.

If distribution of executable or object code is made by offering
access to copy from a designated place, then offering equivalent
access to copy the source code from the same place counts as
distribution of the source code, even though third parties are not
compelled to copy the source along with the object code.

  4. You may not copy, modify, sublicense, or distribute the Program
except as expressly provided under this License.  Any attempt
otherwise to copy, modify, sublicense or distribute the Program is
void, and will automatically terminate your rights under this License.
However, parties who have received copies, or rights, from you under
this License will not have their licenses terminated so long as such
parties remain in full compliance.

  5. You are not required to accept this License, since you have not
signed it.  However, nothing else grants you permission to modify or
distribute the Program or its derivative works.  These actions are
prohibited by law if you do not accept this License.  Therefore, by
modifying or distributing the Program (or any work based on the
Program), you indicate your acceptance of this License to do so, and
all its terms and conditions for copying, distributing or modifying
the Program or works based on it.

  6. Each time you redistribute the Program (or any work based on the
Program), the recipient automatically receives a license from the
original licensor to copy, distribute or modify the Program subject to
these terms and conditions.  You may not impose any further
restrictions on the recipients' exercise of the rights granted herein.
You are not responsible for enforcing compliance by third parties to
this License.

  7. If, as a consequence of a court judgment or allegation of patent
infringement or for any other reason (not limited to patent issues),
conditions are imposed on you (whether by court order, agreement or
otherwise) that contradict the conditions of this License, they do not
excuse you from the conditions of this License.  If you cannot
distribute so as to satisfy simultaneously your obligations under this
License and any other pertinent obligations, then as a consequence you
may not distribute the Program at all.  For example, if a patent
license would not permit royalty-free redistribution of the Program by
all those who receive copies directly or indirectly through you, then
the only way you could satisfy both it and this License would be to
refrain entirely from distribution of the Program.

If any portion of this section is held invalid or unenforceable under
any particular circumstance, the balance of the section is intended to
apply and the section as a whole is intended to apply in other
circumstances.

It is not the purpose of this section to induce you to infringe any
patents or other property right claims or to contest validity of any
such claims; this section has the sole purpose of protecting the
integrity of the free software distribution system, which is
implemented by public license practices.  Many people have made
generous contributions to the wide range of software distributed
through that system in reliance on consistent application of that
system; it is up to the author/donor to decide if he or she is willing
to distribute software through any other system and a licensee cannot
impose that choice.

This section is intended to make thoroughly clear what is believed to
be a consequence of the rest of this License.

  8. If the distribution and/or use of the Program is restricted in
certain countries either by patents or by copyrighted interfaces, the
original copyright holder who places the Program under this License
may add an explicit geographical distribution limitation excluding
those countries, so that distribution is permitted only in or among
countries not thus excluded.  In such case, this License incorporates
the limitation as if written in the body of this License.

  9. The Free Software Foundation may publish revised and/or new versions
of the General Public License from time to time.  Such new versions will
be similar in spirit to the present version, but may differ in detail to
address new problems or concerns.

Each version is given a distinguishing version number.  If the Program
specifies a version number of this License which applies to it and "any
later version", you have the option of following the terms and conditions
either of that version or of any later version published by the Free
Software Foundation.  If the Program does not specify a version number of
this License, you may choose any version ever published by the Free Software
Foundation.

  10. If you wish to incorporate parts of the Program into other free
programs whose distribution conditions are different, write to the author
to ask for permission.  For software which is copyrighted by the Free
Software Foundation, write to the Free Software Foundation; we sometimes
make exceptions for this.  Our decision will be guided by the two goals
of preserving the free status of all derivatives of our free software and
of promoting the sharing and reuse of software generally.

			    NO WARRANTY

  11. BECAUSE THE PROGRAM IS LICENSED FREE OF CHARGE, THERE IS NO WARRANTY
FOR THE PROGRAM, TO THE EXTENT PERMITTED BY APPLICABLE LAW.  EXCEPT WHEN
OTHERWISE STATED IN WRITING THE COPYRIGHT HOLDERS AND/OR OTHER PARTIES
PROVIDE THE PROGRAM "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THE ENTIRE RISK AS
TO THE QUALITY AND PERFORMANCE OF THE PROGRAM IS WITH YOU.  SHOULD THE
PROGRAM PROVE DEFECTIVE, YOU ASSUME THE COST OF ALL NECESSARY SERVICING,
REPAIR OR CORRECTION.

  12. IN NO EVENT UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING
WILL ANY COPYRIGHT HOLDER, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR
REDISTRIBUTE THE PROGRAM AS PERMITTED ABOVE, BE LIABLE TO YOU FOR DAMAGES,
INCLUDING ANY GENERAL, SPECIAL, INCIDENTAL OR CONSEQUENTIAL DAMAGES ARISING
OUT OF THE USE OR INABILITY TO USE THE PROGRAM (INCLUDING BUT NOT LIMITED
TO LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY
YOU OR THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER
PROGRAMS), EVEN IF SUCH HOLDER OR OTHER PARTY HAS BEEN ADVISED OF THE
POSSIBILITY OF SUCH DAMAGES.

		     END OF TERMS AND CONDITIONS
*/