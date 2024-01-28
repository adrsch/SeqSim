using SEQ.Script;
using SEQ.Script.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Sim
{
    public class SimModule : ISeqModule
    {
        public int Priority { get; } = -1;
        public void Init()
        {
            Shell.Add(SimCommands.Commands);
            FactionManager.S = new FactionManager();
            MovementState.Inst = new MovementState();
            PlayerData.Init();
        }

        public void Exit()
        {

        }
    }
}
