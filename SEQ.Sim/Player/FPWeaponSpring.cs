using SEQ;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class LandedEvent
    {

    }
    public class JumpedEvent
    {

    }
    public class FPWeaponSpring : SyncScript
    {
         TransformComponent View;
         PlayerController Player;
        Quaternion Derivative;
        Quaternion Current;
        public float SmoothTime = 0.3f;
      //  public float MaxAngle = 5f;
        public float MaxSmoothedAngle = 8f;
        public float LandingImpulse = -0.4f;

        bool ApplyVerticalForce;
        public float PositionSmoothTime = 0.2f;
        public float SwayTimeScale = 4;
        public float SwayMagnitude = 0.005f;
        public float UpSwayTimeFactor = 2f;
        public float UpSwayMagnitude = 0.005f;
        public float MinimumSwaySpeed = 10f;
        public float MinimumTurnAngle = 1f;
        Vector3 CurrentVelocity;
        Vector3 PositionOffset;
        public event System.Action OnFPSpringUpdate;
        public static FPWeaponSpring S;
        public float SwayAndSlowdownAmount = 0.8f;

        float Amount = 1f;
        public float AmountSmoothTime = 0.2f;
        float AmountVel;

        public void OnShoot()
        {
            Amount *= 0.5f;
        }
        public override void Start()
        {
            base.Start();
            S = this;
            EventManager.AddListener<LandedEvent>(evt => ApplyVerticalForce = true);
            EventManager.AddListener<JumpedEvent>(evt => ApplyVerticalForce = true);

            //         foreach (var fpi in Entity.GetInterfacesInChildren<IFpSpringInit>())
            //       {
            //         fpi.OnFPInit();
            //   }
          //  Player.AfterPhysicsUpdate += DoUpdate;
        }

        // Update is called once per frame

        public override void Update()
        {
            Player = PlayerController.S;
            View = PlayerController.S.FPSCam.Transform;
            if (Player == null || View == null)
                return;

            ///  if (!RequireFPState || MovementState.Inst.IsActive())
            //   {
            View.UpdateWorldMatrix();
                var signedAngle = Vector3.SignedAngle(View.Forward, Transform.Forward, Vector3.down);
                var isPlayerMoving = (MathF.Abs(Player.Velocity.x) + MathF.Abs(Player.Velocity.z)) > MinimumSwaySpeed;
                if (ApplyVerticalForce)
                {
                    CurrentVelocity = new Vector3(CurrentVelocity.x, LandingImpulse, CurrentVelocity.z);
                }
                var targ = !Player.IsGrounded ? Vector3.zero : (
                (isPlayerMoving ? 1 : 0) * ((Transform.Right) * MathF.Sin(Time.time * SwayTimeScale) * SwayMagnitude + Transform.Up * UpSwayMagnitude * MathF.Cos(Time.time * SwayTimeScale * UpSwayTimeFactor))
                );
                PositionOffset = Vector3.CriticalDamp(PositionOffset, targ, ref CurrentVelocity, PositionSmoothTime, Time.deltaTime);

                Transform.WorldPosition = View.WorldPosition + SwayAndSlowdownAmount * PositionOffset;

            //   Transform.UpdateWorldMatrix();

             var smoothAngle = Vector3.Angle(Current * Vector3.forward, View.Back);
      //      if (smoothAngle > 180)
        //        smoothAngle -= 180;
          //  else if (smoothAngle < -180)
            //    smoothAngle += 180;

       ///     var smoothAngle = Vector3.Angle(Transform.Forward, View.Forward);

            if (smoothAngle > MaxSmoothedAngle * Amount)
                {
                    var toRotate = smoothAngle - MaxSmoothedAngle * Amount;
                // Current = Quaternion.RotateTowards(Current, View.WorldRotation, toRotate);
                Current = Quaternion.Slerp(View.WorldRotation, Current, MaxSmoothedAngle * Amount / smoothAngle);

            }

            Transform.UpdateWorldMatrix();
            Current = QuaternionUtil.SmoothDamp(Current, View.WorldRotation, ref Derivative, SwayAndSlowdownAmount * SmoothTime, Time.deltaTime);
            Transform.Rotation = Current;
            Transform.UpdateWorldMatrix();
            /*
            var deltaAngle = Vector3.Angle(Transform.Forward, View.Forward);
            if (deltaAngle > MaxAngle * Amount)
            {
                var toRotate = deltaAngle - MaxAngle * Amount;
                // Transform.Rotation = Quaternion.RotateTowards(Transform.Rotation, View.WorldRotation, toRotate);
                Current = Quaternion.Slerp(View.WorldRotation, Current, MaxAngle * Amount / deltaAngle);

            }

            Transform.UpdateWorldMatrix();*/

            //  var localDelta = View.InverseTransformDirection(transform.forward - View.forward);
            //var localDelta = View.WorldToLocal(Transform.Forward - View.Forward);


            /*
                Shader.SetGlobalVector(GlobalShaderVarSetter.FPSpring, new Vector4(
                    localDelta.x, localDelta.y,
                    //	Vector3.SignedAngle(transform.forward, View.forward, Vector3.up) / MaxAngle,
                    //	Vector3.SignedAngle(transform.forward, View.forward, transform.right) / MaxAngle,
                    0, 0));
            */

            ///	Mecha.SetLocation(transform);
            //Mecha.SetAnimatorState(new MechaAnimatorState { IsGrounded = Player.IsGrounded || ApplyVerticalForce, WorldVelocity = Player.Velocity, SignedTurnAngle = signedAngle });
            ApplyVerticalForce = false;
                OnFPSpringUpdate?.Invoke();
        //    }

            Amount = MathUtil.CriticalDamp(Amount, 1f, ref AmountVel, AmountSmoothTime, Time.deltaTime);
            //Transform.Position = View.WorldPosition;
           // Transform.Rotation = View.WorldRotation;
        }
        /*
        public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;
            return Quaternion.Euler(
              //     MathF.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
              //   MathF.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
              //  MathF.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)

              MathUtil.CriticalDamp(c.x, t.x, ref currentVelocity.x, smoothTime),
              MathUtil.CriticalDamp(c.y, t.y, ref currentVelocity.y, smoothTime),
              MathUtil.CriticalDamp(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }*/

    }

    /*
	Copyright 2016 Max Kaufmann (max.kaufmann@gmail.com)
	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	*/

    public static class QuaternionUtil
    {

        /*
        public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel)
        {
            var Spin = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f);
            var Result = Spin * Current;
            return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
        }

        public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv)
        {
            var Result = Deriv * Quaternion.Invert(Current);
            return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
        }

        public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime)
        {
            if (DeltaTime < Mathf.Epsilon) return Rotation;
            var Deriv = AngVelToDeriv(Rotation, AngularVelocity);
            var Pred = new Vector4(
                    Rotation.x + Deriv.x * DeltaTime,
                    Rotation.y + Deriv.y * DeltaTime,
                    Rotation.z + Deriv.z * DeltaTime,
                    Rotation.w + Deriv.w * DeltaTime
            ).normalized;
            return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
        }
        */
        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time, float dt)
        {
            if (Time.deltaTime < float.Epsilon) return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                MathUtil.CriticalDamp(rot.x, target.x, ref deriv.X, time, dt),
                MathUtil.CriticalDamp(rot.y, target.y, ref deriv.Y, time, dt),
                MathUtil.CriticalDamp(rot.z, target.z, ref deriv.Z, time, dt),
                MathUtil.CriticalDamp(rot.w, target.w, ref deriv.W, time, dt)
            );
            Result.Normalize();

            /*
            // ensure deriv is tangent
            //   var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            var derivError = ProjectToNormal(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);

            deriv.x -= derivError.X;
            deriv.y -= derivError.Y;
            deriv.z -= derivError.Z;
            deriv.w -= derivError.W;
            */
            return new Quaternion(Result.X, Result.Y, Result.Z, Result.W);
        }

        static Vector4  ProjectToNormal(Vector4 u, Vector4 v)
        {
            var backoff = Vector4.Dot(u, v);
            if (backoff < 0)
            {
                backoff *= 1.001f;
            }
            else
            {
                backoff /= 1.001f;
            }

            var change = v * backoff;
            return new Vector4(
                   u.X - change.X,
                   u.Y - change.Y,
                   u.Z - change.Z,
                    u.W - change.W);
        }
    }
}