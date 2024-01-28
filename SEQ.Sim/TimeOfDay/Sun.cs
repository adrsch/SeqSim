using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering.Lights;
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
    public class Sun : SyncScript
    {
        float CurrentSunAngle;
        float Velocity;
        public float SmoothTime = 0.2f;



        public override void Update()
        {
            CurrentSunAngle = MathUtil.CriticalDamp(CurrentSunAngle, Clock.S.TargetSunAngle, ref Velocity, SmoothTime, Time.deltaTime);
            Transform.RotationEulerXYZ = new Vector3(
                CurrentSunAngle,
                0, 0);
        }
    }
}
