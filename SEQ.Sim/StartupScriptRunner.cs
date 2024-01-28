using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

// TODO remove this
namespace SEQ.Sim
{
    public class StartupScriptRunner : AsyncScript
    {
        public override async Task Execute()
        {
            //   await Task.Delay(20000);
            GameStateManager.Push(MovementState.Inst);
        }

    }
}