using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class SimDamageable : StartupScript, IDamageable, ICvarListener
    {
        //public Faction Faction;
        public Actor Actor;

        public event Action ResetAction;

        public event Action<DamageInfo> DamagedAction;

        public event Action DieAction;
        public event Action<DamageInfo> DieDamageAction;

        string hpStrngCache;
        public override void Start()
        {
            ResetAction?.Invoke();
            hpStrngCache = $"{Actor.State.SeqId}:hp";
            UpdateFromState();


            if (!string.IsNullOrWhiteSpace(Actor.State.SeqId))
            {
                Actor.State.AddListener(new CvarListenerInfo
                {
                    Listener = this,
                    OnValueChanged = UpdateFromState,
                });
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Error, $"SimDamageable {Entity.Name} {Actor.State.SeqId} on a thing with no id");
            }
        }

        [DataMember] bool _Penetrable;
        public bool Penetrable => _Penetrable;

        [DataMember] bool _UsesKnockback;
        public bool UsesKnockback => _UsesKnockback;

        [DataMember] TransformComponent _HitboxCenter;
        public Vector3 HitboxCenter => _HitboxCenter.WorldPosition;

        Vector3 IDamageable.HitboxCenter => throw new System.NotImplementedException();

        bool IDamageable.IsDead => throw new System.NotImplementedException();

        public bool IsDead;

        public bool IsDamaged => GetDataHp() < 50;

        public void Damage(DamageInfo info)
        {
            Cvars.Set(hpStrngCache, (GetDataHp() - info.Amount).ToString());
            //  Entity.State.Vars[HealthKey] = (GetDataHp() - info.Amount).ToString();
            if (!IsDead)
            {
                DamagedAction?.Invoke(info);
            }
            else
            {
                DieDamageAction?.Invoke(info);
            }
        }
        public const string HealthKey = "hp";
        int GetDataHp()
        {
            return Actor.State.GetVar<int>(HealthKey);
        }
        void UpdateFromState()
        {
            var dataHp = GetDataHp();
            if (dataHp <= 0)
            {
                if (!IsDead)
                {
                    IsDead = true;
                    DieAction?.Invoke();
                }
            }
            else
            {
                if (IsDead)
                {
                    IsDead = false;
                    ResetAction?.Invoke();
                }
            }
        }

        void OnDestroy()
        {
            Actor.State.RemoveListener(this);
        }
    }
}