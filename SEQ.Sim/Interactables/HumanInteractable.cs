using Stride.Core;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class HumanInteractable : InteractableBase, IFocusText
    {
        public FactionAI AI;

        public string Name;

        public override void Activate()
        {
            if (!AllowInteraction())
                return;

            /*
            if (IsCvarItem)
            {
                var amt = Actor.State.Quantity;
                Cvars.EvalAndSet($"player:{Cvar}", $"player:{Cvar} + {amt}");
                Actor.DestroyWorld();
            }
            else
            {
                PlayerData.TryAddEntityToInventory(Actor);
            }
            */
        }
        public virtual bool AllowInteraction() { return AI.Faction.CanTalk(PlayerAnimator.S.Faction); }

        public override void Deactivate(bool focused)
        {
        }

        public override void Focus()
        {
            if (!AllowInteraction())
                return;
        }

        public string GetText()
        {
            if (!G.S.DebugMode)
                return $"{Name}\n{AI.Faction}";
            else return $"{Name}\n{AI.Faction}\n{AI.Actor.State.SeqId}"
                + $"\nAnimState: {AI.Animator.State}  Idling: {AI.Idling} Speed {AI.GetSpeed()}"
                + $"\nMood: {AI.Mood}  Order: {AI.Order}"
                + $"\nFireDown: {AI.FireDown} Bust: {AI.Burst} Reloading: {AI.Animator.Reloading}"
                + $"\nTarget: {(AI.Target)} EffectiveTarget: {AI.EffectiveTarget}"
                + $"\nFortitude:{AI.Fortitude} Recklessness:{AI.Recklessness}"
                ;
        }

        public override void Unfocus(bool activated)
        {
        }
    }
}