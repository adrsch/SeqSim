using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Animations;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class PlayerCamera : SeqCam
    {
        public static PlayerCamera Sim => S as PlayerCamera;
        public CameraComponent FP;
        public ComputeCurveSamplerVector2 RecoilCurve = new ComputeCurveSamplerVector2();
    }
}
