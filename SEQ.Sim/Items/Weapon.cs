using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class Weapon : ActorUsable
    {
        public FireManagerBase GetShootManager() => ShootManager;
        public FullAutoFire ShootManager = new FullAutoFire();

        public AmmoManagerBase GetAmmoManager() => AmmoManager;
        public MagAmmoManager AmmoManager = new MagAmmoManager();


        public ProjectileManager ProjectileManager = new ProjectileManager();
        public ProjectileManager GetProjectileManager() => ProjectileManager;
        public TransformComponent GetMuzzleTransform() => Animator.MuzzleTransform;

        public WeaponAnimator Animator;

        public ActorState GetOwner()
        {
            return Bound.GetParent();
        }

        public ActorState GetBound() => Bound;

        public bool GetIsReady() => IsReady;

      //  public MonoBehaviour AsMB() => this;


       // protected abstract void AddExtraCvars(List<CvarMultiListenerInfo> cvars);

        public override void OnInit()
        {
            base.OnInit();
            ShootManager.Weapon = this;
            AmmoManager.Weapon = this;
            ProjectileManager.Weapon = this;
            Animator = Child.GetInChildren<WeaponAnimator>();

            ProjectileManager.DoInit();
            ResetWeapon();
        }
        public void ResetWeapon()
        {
            OnResetWeapon();
        }

        protected virtual void OnResetWeapon()
        {
            ShootManager.ResetWeapon();
            AmmoManager.ResetWeapon();
            ProjectileManager.ResetWeapon();
        }

        public override bool Reload()
        {
            return AmmoManager.TryReload();
        }

        public override void OnEquip()
        {
            User.Bind(this);
            AmmoManager.Owner = GetOwner();
            ShootManager.Owner = GetOwner();
            ProjectileManager.Owner = GetOwner();
            AmmoManager.User = User;
            ShootManager.User = User;
            ProjectileManager.User = User;
            Animator.Equip();
            //  CrosshairManager.Inst.SwitchTo(CrosshairPreset);
        }

        public override void OnUnequip()
        {
            Animator.Unequip();
            User.OnUnequip();
            AmmoManager.OnBeginUnequip();
        }

        public override void FinishedUnequip()
        {
           // CrosshairManager.Inst.SwitchTo("default");
            AmmoManager.OnUnequip();
            ShootManager.OnUnequip();
            base.FinishedUnequip();

        }
        public virtual bool CanShoot()
        {
            if (!MovementState.Inst.IsActive())
                return false;
            return true;
        }

        public bool HasAmmo()
        {
            return !AmmoManager.IsEmpty();
        }

        public override void OnFireDown()
        {
            ShootManager.OnFireDown();
        }
        public override void OnFireFrame()
        {
            ShootManager.OnFireFrame();
        }

        public override void OnFireUp()
        {
            ShootManager.OnFireUp();
        }


        public override void OnAltFireDown()
        {
            //  ShootManager.OnFireDown();
        }

        public override void OnAltFireFrame()
        {
            //  ShootManager.OnFireDown();
        }

        public override void OnAltFireUp()
        {
            //  ShootManager.OnFireDown();
        }
    }
}
