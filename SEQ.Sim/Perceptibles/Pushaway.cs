using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Sim
{
    public static class Pushaway
    {
        public static Vector3 Process(IPerceptible actor)
        {
            var position = actor.GetPosition();
            Vector3 velocity = Vector3.zero;
            foreach (var p in World.Current.Perceptibles)
            {
                if (p != actor)
                {
                    var delta = p.GetPosition() - position;
                    var magnitude = delta.Magnitude;

                    if (magnitude < 2)
                    {
                        velocity -= ( delta.Normalized * 2 - magnitude ) * 0.25f;
                    }
                }
            }
            return velocity;
        }
    }
}
