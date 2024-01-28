using System.Collections;
using System.Collections.Generic;
using System;
using Stride.Engine;
using Stride.Core.Mathematics;
using Silk.NET.SDL;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public interface ITargetFilter
    {
        bool Accept(IPerceptible target);
    }

    public class AcceptAllFilter : ITargetFilter
    {
        public bool Accept(IPerceptible targ) => true;
    }

    public class AcceptRelationsFilter : ITargetFilter
    {
        public IFactionProvder faction;
        public Func<RelationType, bool> relationFilter;
        public bool Accept(IPerceptible targ)
        {
            if (relationFilter != null)
                return relationFilter(FactionManager.GetRelations(faction).Relation(targ.Faction));
            else
                return true;
        }
    }
    public interface ISensorTarget : IWorldObject
    {
        int SensorTargetPriority { get; }
        IFactionProvder GetFaction();
        IPerceptible Perceptible { get; set;}

        TransformComponent NavTarget { get; }
    }
    public static class SensorTargetExtensions
    {
        public static Vector3 NavPosition(this ISensorTarget t) => t.NavTarget?.WorldPosition ?? t.Position;
        public static SimDamageable GetSimDamageable(this ISensorTarget t) => (t.NavTarget != null ? t.NavTarget : (t as ScriptComponent).Transform).Entity.GetInParent<SimDamageable>();
        public static SimDamageable GetSimDamageable(this PhysicsComponent pc) => pc.Entity.GetInParent<SimDamageable>();
        public static IPerceptible GetFactionObject(this PhysicsComponent pc) => pc.Entity.GetInterfaceInParent<IPerceptible>();

        public static Vector3 SensorPosition(this ISensor s, bool recalc) => (s as ScriptComponent).Transform.GetWorldPosition(recalc);
    }
    public class DetectionReport
    {
        public Dictionary<ISensorTarget, float> Detected = new();
    }

    public class PerceptibleDetection
    {
        public ISensorTarget HighestPriority;
        public ISensor SpottingSensor;
        public float Strength;
        public float StrengthThisUpdate;
        public float StrengthVelocity;
        public bool HighValueTarget;
    }


    public class SensorAggregator
    {
        public ITargetFilter filter;
        public List<ISensor> sensors;
        public ISensorTarget Target { get; private set; }
        public IPerceptible TargetPerceptible;
        public ISensor SpottingSensor { get; private set; }
        public IFactionProvder PreferredFaction;// = Faction.none;

        public Dictionary<IPerceptible, PerceptibleDetection> Detected = new();

        public PerceptibleDetection Get(IPerceptible p)
        {
            if (Detected.TryGetValue(p, out var result))
                return result;
            Detected[p] = new PerceptibleDetection();
            return Detected[p];
        }

        public SensorAggregator(List<ISensor> sensors)
        {
            this.sensors = sensors;
            filter = new AcceptAllFilter();
        }

        public SensorAggregator(List<ISensor> sensors, IFactionProvder f)
        {
            this.sensors = sensors;
            //    filter = new AcceptRelationsFilter { faction = f };

            filter = new AcceptAllFilter();
        }
        public IPerceptible This;

        // TODO this is tiem stuff
        public void UpdateDetections()
        {
            foreach (var d in Detected.Values)
            {
                d.StrengthThisUpdate = 0;
                d.HighestPriority = null;
                d.SpottingSensor = null;
            }

            var tPriority = float.MinValue;
            Target = null;
            SpottingSensor = null;
            foreach (var sensor in sensors)
            {
                sensor.UpdateDetections(this);

            }

            foreach (var d in Detected)
            {
                d.Value.Strength = d.Value.Strength.CriticalDamp(d.Value.StrengthThisUpdate, ref d.Value.StrengthVelocity, 1f, Time.deltaTime);

                if (d.Value.Strength > 0.001f)
                {
                    var rel = (This.Faction.GetRelation(d.Key.Faction));
                    if (rel == RelationType.Violence || rel == RelationType.Unsure
                        || (rel == RelationType.Neutral && d.Key.IsArmed))
                    {
                        var effectivePriority 
                            = d.Value.Strength 
                            + (d.Key.IsArmed ? 0.5f : 0) 
                            + (TargetPerceptible == d.Key ? 1f : 0)
                            + (d.Value.HighValueTarget ? 1f : 0);
                        if (effectivePriority > tPriority && filter.Accept(d.Key))
                        {
                            tPriority = effectivePriority;
                            Target = d.Value.HighestPriority;
                            TargetPerceptible = d.Key;
                            SpottingSensor = d.Value.SpottingSensor;
                        }
                    }
                }
            }

            /*
                foreach (var detection in report.Detected)
                {
                    var effectivePriority = detection.SensorTargetPriority + (detection.GetFaction() == PreferredFaction ? 10 : 0);
                    if (effectivePriority > tPriority && filter.Accept(detection))
                    {
                        tPriority = effectivePriority;
                        Target = detection;
                        SpottingSensor = sensor;
                    }
                }
            }*/
        }
    }
    public interface ISensor
    {

        void UpdateDetections(SensorAggregator sa);
    }
}