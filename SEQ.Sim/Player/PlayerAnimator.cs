using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stride.Graphics.GeometricPrimitives.GeometricPrimitive;
using SEQ.Script;

namespace SEQ.Sim
{
    public class PlayerAnimator : StartupScript, IUsableUser, IPerceptible
    {
        public static PlayerAnimator S;

        public PerceptibleStatus Status => Damageable.IsDead ? PerceptibleStatus.Dead : Damageable.IsDamaged ? PerceptibleStatus.Hurt : PerceptibleStatus.Default;
        public override void Start()
        {
            base.Start();
            S = this;
            World.RegisterPerceptible(this);
        }
        public override void Cancel()
        {
            base.Cancel();
            World.RemovePerceptible(this);
        }

        [DataMemberIgnore]
        public List<VisualEmitter> VisualPerceptibles { get; set; } = new List<VisualEmitter>();

        public bool IsUnequipping;
        public Entity ParentEnt => FPWeaponSpring.S.Entity;

        public SimDamageable Damageable;

        public event Action<Weapon> OnShootEvent;
        public AudioEmitterComponent PlayerAudioEmitter;
        public event Action OnUnequipEvent;

        public AudioEmitter AudioPerceptible { get; set; }

        public IFactionProvder Faction { get; set; }

        public bool IsArmed => CurrentWeapon != null;

        Weapon CurrentWeapon;
        public void Bind(Weapon weapon)
        {
            CurrentWeapon = weapon;
        }

        public AudioEmitterComponent AudioEmitter()
        {
            return PlayerAudioEmitter;
        }

        public void OnReloadEnd()
        {
            CrosshairManager.IsReloading = false;
        }

        public void OnReloadStart(bool isEmpty)
        {
            CrosshairManager.IsReloading = true;
        }

        public void OnReloadNoAmmo()
        {
        }

        public void OnFireStart()
        {
            FPWeaponSpring.S.OnShoot();
            OnShootEvent?.Invoke(CurrentWeapon);
        }

        public void OnFireEnd()
        {
        }

        public void OnShoot(bool holdOpen)
        {
            FPWeaponSpring.S.OnShoot();
            OnShootEvent?.Invoke(CurrentWeapon);
            AudioPerceptible.Impulse(CurrentWeapon.Species.FireDecibles);
        }

        public void OnUnequip()
        {
            OnUnequipEvent?.Invoke();
            CurrentWeapon = null;
        }

        public float GetSpread()
        {
            return CrosshairManager.GetAccuracySpread();
        }

        public Vector3 GetShootPosition()
        {
            return PlayerCamera.Sim.FP.Transform.WorldPosition;
        }

        public Vector3 GetShootForward()
        {
            return PlayerCamera.Sim.FP.Transform.Forward;
        }

        public bool IsPlayer()
        {
            return true;
        }

        public TransformComponent Feet;
        public Vector3 GetPosition()
        {
            return Feet.Transform.WorldPosition;
        }
    }
}
