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
    public class NavTester : AsyncScript
    {
        public NavMeshAgent agent;

        public override async Task Execute()
        {
            while (true)
            {
                if (PlayerController.S != null)
                {
                    agent.SetDestination(PlayerController.S.Entity.Transform.WorldPosition);
                }
                await Task.Delay(3000);
            }
        }
    }
}
