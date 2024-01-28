using Stride.Engine;
using Stride.Rendering;
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
    public class AIRandomizer : ScriptComponent
    {
        public ModelComponent Model { get; set; }
        public List<Material> HeadVariants = new List<Material>();
        public int HeadIndex;
        public List<Material> UpperVariants = new List<Material>();
        public int UpperIndex;
        public List<Material> LowerVariants = new List<Material>();
        public int LowerIndex;
        public List<Material> ShoeVariants = new List<Material>();
        public int ShoeIndex;


        public int Generate()
        {
            var rand = Random.Shared.Next(HeadVariants.Count);
            return rand;
        }

        public void Set(int index, List<Material> list, int rand)
        {
            if (list.Count == 0)
                return;
            if (rand >= list.Count)
               // || index >= Model.Materials.Count)
            {
                Logger.Log(Channel.Gameplay, LogPriority.Warning, $"Index for randomizer bigger than list");
            }
            Model.Materials[index] = list[rand];
        }
    }
}
