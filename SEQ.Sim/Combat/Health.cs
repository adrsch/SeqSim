using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public struct DamageInfo
    {
        public int Amount;
        public float Knockback;
        public IFactionProvder Faction;
        public Vector3 Point;
        public Vector3 Forward;
        public Vector3 Normal => -Forward;
        public SurfaceType Effect;
        public IPerceptible Perpetrator;
    }
    public interface IDamageable
    {
        bool Penetrable { get; }
        void Damage(DamageInfo info);
        bool UsesKnockback { get; }
        Vector3 HitboxCenter { get; }
        bool IsDead { get; }

    }

    [System.Serializable]
    public class Health
    {
        float _Hp;
        public float Hp
        {
            get => _Hp; set
            {
                if (value > 0)
                    Dead = false;
                _Hp = value;
            }
        }

        public float HealthPercent => Hp / MaxHp;

        public float MaxHp;
        public event Action OnDie;
        public event Action<DamageInfo> OnDamaged;
        public bool Dead { get; private set; }

        public void SetToMax()
        {
            Hp = MaxHp;
            if (Hp > 0)
                Dead = false;

        }

        public void Damage(DamageInfo info)
        {
            Hp -= info.Amount;
            OnDamaged?.Invoke(info);
            if (!Dead && Hp <= 0)
                Die();
        }

        void Die()
        {
            Dead = true;
            OnDie?.Invoke();
        }
    }
}