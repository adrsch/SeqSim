using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public static class Units
    {
        //public const float QuakeHeight = 56f
        // public const float QuakeHeight = 48f;

        //public const float QuakeHeight = 72;
        public const float QuakeHeight = 56;
        public static float UnityHeight = 1.8f;
        public static float ToKPH(float ms) => ms * 3.6f;
        public static float ToMS(float kph) => kph / 3.6f;


    }
}
