using Stride.Animations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI.Controls;
using System;
using SEQ.Script;
using SEQ.Sim;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum CrosshairPreset
    {
        Default = 0,
        Food = 1,
        Rifle = 2,
    }
    public class CrosshairManager
    {
        public UIComponent UI;
        public ImageElement Center;
        public ImageElement Right;
        public ImageElement Left;
        public ImageElement Up;
        public ImageElement Down;

        float CurrentLength = 4;
        public float TargetLength = 4;

        const float Thickness = 4;//3; //4
        const float DotSize = 4; //6; //4;
        const float HalfDot = 2; //3; //2

        float CurrentSpread = 4;
        public float TargetSpread = 4;

        float LengthVel;
        float SpreadVel;

        public bool ShowCenter = true;

        public void OnStart()
        {
            ActorSpeciesRegistry.S.ResetSpawns += () => HasInit = false;
        }
        bool HasInit; 

        public void Preset(CrosshairPreset preset)
        {

        }

        float lastShootTime;
        int ShootCounter;

        Vector2 SampleDamage()
        {
            return new Vector2(DamageOffset.X + (Random.Shared.NextSingle() * 2 - 1) ,
                DamageOffset.Y + (Random.Shared.NextSingle() * 2 - 1) );

        }
        Vector2 SampleRecoil(int p)
        {
            var keyframes = (PlayerCamera.Sim.RecoilCurve.Curve as ComputeAnimationCurveVector2).KeyFrames;

            var frameCount = p % keyframes.Count;
            return keyframes[frameCount].Value;

        }
        public void DoUpdate(float dt)
        {
            if (!HasInit && PlayerController.S != null && PlayerAnimator.S != null)
            {

                PlayerController.S.JumpedEvent.Event += () =>
                {
                    PerturbTime = Time.time;
                    PerturbSpread = 32;
                    PerturbLength = 0;
                    PerturbDuration = 0.25f;
                };

                PlayerController.S.OnPush += () =>
                {
                    PerturbTime = Time.time;
                    var amt = MathUtil.Clamp01((PlayerController.S.CurrentVelocity.Magnitude - 50f) / 70f);
                    PerturbSpread = MathUtil.Lerp(6f, 28f, amt);
                    PerturbDuration = 0.5f;
                    PerturbLength = MathUtil.Lerp(8f, 24, amt);
                };

                PlayerAnimator.S.OnUnequipEvent += () => ShootCounter = 0;

                PlayerAnimator.S.OnShootEvent += weapon =>
                {
                   // float camerascaler = 1f;
                    if (Time.time > lastShootTime + 0.6f)
                    {
                        ShootCounter = 0;
                    }
                    else if (Time.time > lastShootTime + 0.3f)
                    {
                        ShootCounter -= 2;
                        if (ShootCounter < 0) ShootCounter = 0;
                        if (ShootCounter > 4) ShootCounter = 4;
                      //  camerascaler = MathUtil.Clamp01((Time.time - lastShootTime) / weapon.Species.FireResetAuto);
                    }
                    var add = 1f - MathUtil.Clamp01(Time.time - lastShootTime);
                    PerturbTime = Time.time;
                    var sp = MathF.Max(TargetSpread * 2, 24f);
                    var repeatInaccuracy = MathUtil.Clamp01((float)ShootCounter / 6f);
                    CameraOffset = SampleRecoil(ShootCounter);
                    CameraPerturbTime = Time.time;
                    CurrentSpread = MathF.Min(sp + repeatInaccuracy * 70f, 250f);
                    PerturbLength = TargetLength;
                    PerturbDuration = 0.33f;
                    CameraDuration = PerturbDuration * 8f;
                    lastShootTime = Time.time;
                    ShootCounter++;
                };

                PlayerAnimator.S.Damageable.DamagedAction += weapon =>
                {
                    var add = 1f - MathUtil.Clamp01(Time.time - lastShootTime);
                    PerturbTime = Time.time;
                    var sp = MathF.Max(TargetSpread * 2, 24f);
                    DamageOffset = SampleDamage();
                    DamagePerturbTime = Time.time;
                    CurrentSpread = MathF.Min(sp + 15f, 250f);
                    PerturbLength = TargetLength;
                    PerturbDuration = 0.33f;
                    DamageDuration = PerturbDuration * 8f;
                    lastShootTime = Time.time;
                };

                PlayerAnimator.S.OnUnequipEvent += () =>
                {
                    IsReloading = false;
                };

                HasInit = true;
            }

            /*
            Up.Color = Color.Red;
            Left.Color = Color.Red;
            Down.Color = Color.Red;
            Right.Color = Color.Red;
            Center.Color = Color.Red;*/

            if (PlayerController.S == null)
                return;

            var speed = PlayerController.S.CurrentVelocity;
            // var smoothTime = MathUtil.Lerp(0.001f, 0.1f, MathUtil.Clamp01((speed.Magnitude - 70f) / 170f));
            var smoothTime = PlayerController.S.InputManager.IsCancelling() ? 0 : 0.1f;

            UpdateMoveAccuracy(speed);

            Center.Visibility = ShowCenter ? Stride.UI.Visibility.Visible : Stride.UI.Visibility.Hidden;
            CurrentLength = MathUtil.CriticalDamp(CurrentLength, TargetLength, ref LengthVel, smoothTime, dt);
            Left.Width = Left.MaximumWidth = Left.MinimumWidth 
                = Right.Width = Right.MaximumWidth = Right.MinimumWidth 
                = Up.Height = Up.MaximumHeight = Up.MinimumHeight 
                = Down.Height = Down.MaximumHeight = Down.MinimumHeight 
                = CurrentLength;
            Left.Height = Left.MaximumHeight = Left.MinimumHeight 
                = Right.Height = Right.MaximumHeight = Right.MinimumHeight 
                = Up.Width = Up.MaximumWidth = Up.MinimumWidth
                = Down.Width = Down.MaximumWidth = Down.MinimumWidth
                = Thickness;
            Center.MinimumWidth = Center.MaximumWidth = Center.Width = Center.Height = Center.MinimumHeight = Center.MaximumHeight = DotSize;

            Center.Color = Left.Color = Right.Color = Up.Color = Down.Color = Color;
            CurrentSpread = MathUtil.CriticalDamp(CurrentSpread, TargetSpread, ref SpreadVel, smoothTime, dt);
            AccuracySpread = CurrentSpread;
            var halfLen = CurrentLength / 2;
            Left.Margin = new Stride.UI.Thickness(126 - HalfDot - halfLen - CurrentSpread, 0, 126, 0);
            Right.Margin = new Stride.UI.Thickness(0, 0, 126 - HalfDot - halfLen - CurrentSpread, 0);
            Up.Margin = new Stride.UI.Thickness(0, 126 - HalfDot - halfLen - CurrentSpread, 0, 126);
            Down.Margin = new Stride.UI.Thickness(0, 0, 0, 126 - HalfDot - halfLen - CurrentSpread);
        }
        static float AccuracySpread;
        public static float GetAccuracySpread()
        {
            return AccuracySpread / 52f;
        }
        static Vector2 CameraOffset;
        static Vector2 DamageOffset;
        public static Vector2 GetCameraOffset()
        {
            return CameraOffset + DamageOffset * 0.1f ;
        }
        public bool IsUnequipped => PlayerAnimator.S.IsUnequipping;

        Color Color = new Color(255, 255, 255, 255);

        float DamageDuration;
        float DamagePerturbTime;
        float CameraPerturbTime;
        float CameraDuration = 0;
        float PerturbTime;
        float PerturbDuration = 0.25f;
        float PerturbSpread = 45;
        float PerturbLength = 0;

        float InteractSpread = 8;
        float AirSpread = 52; 
        float RunSpread = 32; 
        float RunSpeed = 125; //270
        float DefaultSpread = 4f;
        float DefaultLength = 8f;
        float RunLength = 24f;
        void UpdateMoveAccuracy(Vector3 speed)
        {
            var grounded = PlayerController.S.IsGrounded;

            float spread = CurrentSpread;
            if (!grounded)
            {
                spread = AirSpread;
                if (!IsReloading)
                TargetLength = 32f;
             //   TargetLength = 4f;
            }
            else
            {
                var lerpPos = PlayerController.S.InputManager.IsCancelling()
                    ? MathUtil.Clamp01((speed.Magnitude - 70f) / RunSpeed)
                   : MathUtil.Clamp01((speed.Magnitude - 10f) / RunSpeed);

                if (!IsReloading)
                    TargetLength = MathUtil.Lerp(DefaultLength, RunLength, lerpPos);
               // TargetLength = MathUtil.Lerp(4f, 4f, MathUtil.Clamp01(speed.Magnitude / RunSpeed));

                spread = MathUtil.Lerp(DefaultSpread, RunSpread, lerpPos);
            }

            if (IsReloading)
            {
                PerturbTime = Time.time;
                PerturbSpread = 70;
                PerturbLength = 12f;
                PerturbDuration = 0.25f;
            }
            if (IsReloading )//|| IsUnequipped)
            { 
                Color.A = 150;
            }
            else
            {
                Color.A = 255;
            }

            if (PerturbTime + PerturbDuration > Time.time)
            {
                var amt = (Time.time - PerturbTime) / PerturbDuration;

                spread = MathUtil.Lerp(PerturbSpread, spread, amt);
                if (PerturbLength > 0)
                {
                    TargetLength = MathUtil.Lerp(PerturbLength, TargetLength, amt);
                }
            }
            if (CameraPerturbTime + CameraDuration > Time.time)
            {
                var amt = MathUtil.SmoothStep((Time.time - CameraPerturbTime) / (CameraDuration));

                CameraOffset = new Vector2(
                    MathUtil.Lerp(CameraOffset.X, 0, amt),
                    MathUtil.Lerp(CameraOffset.Y, 0, amt));
            }
            else
                CameraOffset
                    = Vector2.Zero;

            if (DamagePerturbTime + DamageDuration > Time.time)
            {
                var amt = MathUtil.SmoothStep((Time.time - DamagePerturbTime) / (DamageDuration));

                DamageOffset = new Vector2(
                    MathUtil.Lerp(DamageOffset.X, 0, amt),
                    MathUtil.Lerp(DamageOffset.Y, 0, amt));
            }
            else
                DamageOffset
                    = Vector2.Zero;


            if (IsReloading)
            {

            }

            TargetSpread = spread;
        }

        public static bool IsReloading;
    }
}
