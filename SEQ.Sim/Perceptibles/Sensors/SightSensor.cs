using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public interface IAimTarget : IVisionTarget
    {
        Vector3 AimAtPosition { get; }
    }
    public interface IVisionTarget : ISensorTarget
    {

    }
    public class SightSensor : ScriptComponent, ISensor
    {
        // int RaycastLayers => LayerUtil.GetLayers(MaskLayers.Env, MaskLayers.Default);
        float MaxAngle = 135f;
        public void UpdateDetections(SensorAggregator sa)
        {
            foreach (var perceptible in World.Current.Perceptibles)
            {
                foreach (var x in perceptible.VisualPerceptibles)
                {
                    var raycastStart = Entity.Transform.WorldMatrix.TranslationVector;
                    var forward = Entity.Transform.WorldMatrix.Forward;
                    var raycastEnd = x.Position;

                    // var strength = MathUtil.Clamp01(1f - (Vector3.Distance(raycastStart, raycastEnd) - 10f) / 50f);
                    var angle = Vector3.Angle(forward, raycastStart - raycastEnd);
                    if (angle < MaxAngle)
                    {
                        var hit = this.GetSimulation().Raycast(raycastStart, raycastEnd);
                        /*
                         var hit = this.GetSimulation().Raycast(raycastStart, raycastEnd,
                        CollisionFilterGroups.DefaultFilter,
                        CollisionFilterGroupFlags.AllFilter & ~CollisionFilterGroupFlags.CharacterFilter
                        & ~CollisionFilterGroupFlags.AIFilter
                        & ~CollisionFilterGroupFlags.CustomFilter1);
                        */
                        if (!hit.Succeeded || hit.Collider.Entity.GetInterfaceInParent<IPerceptible>() == perceptible)
                        {
                            var entry = sa.Get(perceptible);
                            if (entry.HighestPriority == null || x.SensorTargetPriority > entry.HighestPriority.SensorTargetPriority)
                            {
                                entry.HighestPriority = x;
                                entry.SpottingSensor = this;
                            }
                            entry.StrengthThisUpdate += 1f / perceptible.VisualPerceptibles.Count;
                        }
                    }
                }
            }
            /*
            CurrentDetections.Detected = World.Current.All<IVisionTarget>().Where(x =>
            {
                var raycastStart = Entity.Transform.WorldMatrix.TranslationVector;
                var forward = Entity.Transform.WorldMatrix.Forward;
                var raycastEnd = x.Position;

                var hit = this.GetSimulation().Raycast(raycastStart, raycastEnd,
                    CollisionFilterGroups.DefaultFilter,
                    CollisionFilterGroupFlags.AllFilter & ~CollisionFilterGroupFlags.CharacterFilter
                    & ~CollisionFilterGroupFlags.AIFilter
                    & ~CollisionFilterGroupFlags.CustomFilter1);
                if (!hit.Succeeded || hit.Collider.Entity.GetInterfaceInParent<IVisionTarget>() == x)
                {
                    return true;
                }
                return false;
            });
            */
        }
    }
}