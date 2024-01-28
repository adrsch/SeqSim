using Stride.Core.Mathematics;
using Stride.Engine;
using System.Diagnostics;
using System.Linq;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public interface IProximityTarget : ISensorTarget
    {
        float Decibels { get; }
    }
    public class ProximitySensor : ScriptComponent, ISensor
    {
        public float SensorDistance;
        public bool UseSensorDistance;
        DetectionReport CurrentDetections = new DetectionReport();
       /* void OnDrawGizmos()
        {
            if (Application.isPlaying)
                if (CurrentDetections != null && CurrentDetections.Detected != null)
                    foreach (var detection in CurrentDetections.Detected)
                    {
                        if (detection != null)
                            Debug.DrawLine(transform.position, detection.Position, Color.blue);
                    }
        }
       */
        public void UpdateDetections(SensorAggregator sa)
        {
            foreach (var perceptible in World.Current.Perceptibles)
            {
                var perc = sa.Get(perceptible);

                if (perc.HighestPriority == null ||
                    perceptible.AudioPerceptible.SensorTargetPriority > perc.HighestPriority.SensorTargetPriority)
                {

                    perc.SpottingSensor = this;
                    perc.HighestPriority = perceptible.AudioPerceptible;
                }
                perc.StrengthThisUpdate += perceptible.AudioPerceptible.Decibels *
                                    //    (1 / Vector3.DistanceSquared(Transform.WorldPosition, perceptible.AudioPerceptible.Position));
                                    (1 / Vector3.Distance(Transform.WorldPosition, perceptible.AudioPerceptible.Position));

                // for impulses;
                if (perc.StrengthThisUpdate > perc.Strength)
                    perc.Strength = perc.StrengthThisUpdate;
            }
        }
    }
}