using Stride.Core;
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
    public partial class FactionAI
    {
        [DataMemberIgnore]
        public float Recklessness;
        [DataMemberIgnore]
        public float Fortitude;

        public void GeneratePersonality()
        {
            Recklessness = Random.Shared.NextSingle();
            Fortitude = Random.Shared.NextSingle(); 
        }



        public void EvaluateMood(bool seesThreat)
        {
            if (Dead)
            {
                Mood = MoodType.calm;
            }
            else
            {
                if (TImesShot > 3)
                {
                    if (Order == OrderType.panic)
                        Mood = MoodType.panic;
                    else
                    {
                        var mentalBarrier = CommanderIsDead
                            ? 0.8f : 0.4;
                        if (Fortitude > mentalBarrier)
                            Mood = MoodType.frenzy;
                        else
                            Mood = MoodType.panic;
                    }
                }
                else if (ShotBy != null)
                {
                    var mentalBarrier = CommanderIsDead
                        ? 0.8f : 0.4;
                    if (Recklessness > (
                        CommanderIsDead ? 0.7f : 0.5f))
                    {
                        if (Fortitude > mentalBarrier)
                            Mood = MoodType.determined;
                        else
                            Mood = MoodType.frenzy;
                    }
                    else
                    {
                        if (Fortitude > mentalBarrier)
                            Mood = MoodType.determined;
                        else
                            Mood = MoodType.panic;
                    }
                }
                else if (CommanderIsDead)
                    // could do OR follow target is dead
                {
                    if (Status == PerceptibleStatus.Hurt)
                        Mood = MoodType.panic;
                    else
                        Mood = MoodType.frenzy;
                }
               // else if ()
                else
                {
                    if (Order == OrderType.panic)
                    {
                        Mood = seesThreat ? MoodType.panic : MoodType.calm;
                    }
                }
            }
        }

        public bool CommanderIsDead => Commander != null && Commander.Status == PerceptibleStatus.Dead; 
        public void GiveOrder(IPerceptible from, OrderType order)
        {
            Order = order;
            Commander = from;
        }
    }
}
