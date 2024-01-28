using Stride.Core.Mathematics;
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
    public static class GameplayExtensions
    {
        public static bool IsFacing(this IPerceptible a, Vector3 p) 
        {
            return (a as EntityComponent).Transform.IsFacing(p);
        }

        public static bool IsFacing(this TransformComponent a, Vector3 p)
        {
            var flatFace = new Vector3(p.x, 0, p.z);
            var flatCurrent = new Vector3(a.WorldPosition.x, 0, a.WorldPosition.z);

            var flatForward = new Vector3(a.Forward.x, 0, a.Forward.z);
            var angle = Vector3.Angle(flatCurrent - flatFace, flatForward);

            return angle < 5f;
        }
        public static TransformComponent GetTransform(this ISensorTarget st)
        {
            return (st as EntityComponent)?.Transform;
        }

    }
}
