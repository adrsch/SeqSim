using Stride.Animations;
using Stride.Core;
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
    public class WeaponAnimator : StartupScript
    {
        public AnimationComponent Animations;
        public AudioEmitterComponent Emitter;
        public TransformComponent MuzzleTransform;
        public string HoldClip = "hold";
        public string FireOneshotClip = "fire";
        public string FireLoop = "fireloop";
        public string ReloadClip = "reload";
        public string ReloadNoAmmoClip = "empty";
        public Prefab MuzzleEffect;

        [Display(category: "Weapon", order: 80)]
        public int ReloadMs;

        [Display(category: "Weapon", order: 60)]
        public float FireResetManual;
        [Display(category: "Weapon", order: 70)]
        public float FireResetAuto;
        public override void Start()
        {
            base.Start();
            if (Animations == null) { Animations = Entity.Get<AnimationComponent>(); } 
        }

        public void Equip()
        {
            Animations.PlayIfExists(HoldClip);
        }

        public void Unequip()
        {
            StopReload();
            StopFire();
        }

        public void ReloadNoAmmo()
        {
            Emitter.Oneshot(ReloadNoAmmoClip);
        }

        public void Reload(bool isEmpty)
        {
            Animations.PlayIfExists(ReloadClip);
            Emitter.Startsound(ReloadClip);
        }

        public void ReloadSuccess()
        {
            Animations.BlendIfExists(HoldClip, 1, TimeSpan.FromMilliseconds(100));
        }

        public void FireDown(bool holdOpen)
        {
            StopReload();
            Animations.PlayIfExists(HoldClip);
            Animations.PlayIfExists(FireOneshotClip);
            Animations.PlayIfExists(FireLoop);
            Emitter.Oneshot(FireOneshotClip);
            Emitter.Startsound(FireLoop);
            if (string.IsNullOrEmpty(FireLoop))
            {
                Animations.BlendIfExists(HoldClip, 1, TimeSpan.FromMilliseconds(100));
            }
        }

        public void FireUp()
        {
            StopFire();
        }

        void StopReload()
        {
          //  Animations.BlendIfExists(ReloadClip, 0, TimeSpan.FromMilliseconds(100));
            Animations.BlendIfExists(HoldClip, 1, TimeSpan.FromMilliseconds(100));
            Emitter.Stopsound(ReloadClip);
        }
        void StopFire()
        {
            //Animations.BlendIfExists(FireLoop, 0, TimeSpan.FromMilliseconds(100));
          //  if (!string.IsNullOrEmpty(FireLoop))
           // {
                Animations.BlendIfExists(HoldClip, 1, TimeSpan.FromMilliseconds(100));
           // }
            Emitter.Stopsound(FireLoop);
        }
    }
}
