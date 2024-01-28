
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class ProximityEmitter : StartupScript, IProximityTarget
    {
        public override void Start()
        {
            World.Current.Register(this);
            if (Entity.GetInterfaceInParent<IPerceptible>() is IPerceptible ifact)
            { _Faction = ifact.Faction;
                Perceptible = ifact;
                };
        }
        public IPerceptible Perceptible { get; set; }
        public override void Cancel()
        {
            World.Current?.Remove(this);
            base.Cancel();
        }

       public float _SignalStrength = 3f;

        public IFactionProvder _Faction;
        public IFactionProvder GetFaction()
        {
            return _Faction;
        }
        public int _SensorTargetPriority;
        public int SensorTargetPriority => _SensorTargetPriority;

        [DataMemberIgnore]
        public Vector3 Position { get => Entity.Transform.WorldPosition; set => Entity.Transform.WorldPosition = value; }

        [DataMemberIgnore]
        public Quaternion Rotation { get => Entity.Transform.Rotation; set => Entity.Transform.Rotation = value; }

        [DataMemberIgnore]
        public Vector3 Velocity
        {
            get => Vector3.zero; set
            {
            }
        }

        public TransformComponent NavTarget { get; set; }

        [DataMemberIgnore]
        public float Decibels => _SignalStrength;
    }
}