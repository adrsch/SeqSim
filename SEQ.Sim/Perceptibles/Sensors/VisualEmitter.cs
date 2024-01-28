using System.Collections;
using System.Collections.Generic;
using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Core;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class VisualEmitter : StartupScript, IAimTarget
    {
        public override void Start()
        {
            World.Current.Register(this);
            if (Entity.GetInterfaceInParent<IPerceptible>() is IPerceptible ifact)
            {
                _Faction = ifact.Faction;
                ifact.VisualPerceptibles.Add(this);
                Perceptible = ifact;
            }
        }
        public IPerceptible Perceptible { get; set; }

        public override void Cancel()
        {
            World.Current?.Remove(this);
            base.Cancel();
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

        public Vector3 AimAtPosition => Entity.Transform.WorldPosition;

        public TransformComponent NavTarget { get; set; }

        public IFactionProvder _Faction;
        public IFactionProvder GetFaction()
        {
            return _Faction;
        }
    }
}