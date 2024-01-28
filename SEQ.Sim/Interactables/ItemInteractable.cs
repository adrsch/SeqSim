using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class ItemInteractable : InteractableBase, IFocusText
    {
        public Actor Actor;
        public bool IsCvarItem;
        public string Cvar;

        public override void Activate()
        {
            if (!AllowInteraction())
                return;

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
        }
        public virtual bool AllowInteraction() { return true; }

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
            return Actor.State.GetNameWithQuantity();
        }

        public override void Unfocus(bool activated)
        {
        }
    }
}