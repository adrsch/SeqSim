using System.Collections;
using System.Collections.Generic;
using System;
using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Core;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class AudioEmitter : SyncScript, IProximityTarget
    {
        public override void Start()
        {
            World.Current.Register(this);
            if (Entity.GetInterfaceInParent<IPerceptible>() is IPerceptible ifact)
            {
                _Faction = ifact.Faction;
                ifact.AudioPerceptible = this;
                Perceptible = ifact;
            }
        }
        public IPerceptible Perceptible { get; set; }

        public override void Cancel()
        {
            World.Current?.Remove(this);
            base.Cancel();
        }

        public void Impulse(float db)
        {
            Decibels = db;
            SkipFrame = true;
        }

         float SmoothTime = 5f;
        float V;
         float BaseVolumeDb = 30f;
        bool SkipFrame;
        public override void Update()
        {
            if (SkipFrame)
            {
                SkipFrame = false;
                return;
            }

            Decibels = Decibels.CriticalDamp(BaseVolumeDb, ref V, SmoothTime, Time.deltaTime);
        }


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

        public float Decibels { get; set; }

        [DataMemberIgnore]
        public Vector3 Velocity
        {
            get => Vector3.zero; set
            {
            }
        }

        public TransformComponent NavTarget { get; set; }
    }
}