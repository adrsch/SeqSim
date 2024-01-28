
using Stride.Engine;
using System.Collections;
using System.Collections.Generic;
using Stride.Core.Mathematics;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum PerceptibleStatus
    {
        Default,
        Dead,
        Hurt,
    }
    public interface IPerceptible
    {
        IFactionProvder Faction { get; set; }
        bool IsArmed { get; }
        PerceptibleStatus Status { get; }
        Vector3 GetPosition();

        AudioEmitter AudioPerceptible { get; set; }
        List<VisualEmitter> VisualPerceptibles { get; set; }
    }

}