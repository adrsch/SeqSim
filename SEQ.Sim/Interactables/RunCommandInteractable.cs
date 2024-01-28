
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{

    public class RunCommandInteractable : InteractableBase, IInteractable, IFocusText
    {
        public string LocKey;
        public List<string> Commands = new List<string>();
        public EventWrapper InteractEvent = new EventWrapper();

        
        public EventWrapper OnFocus = new EventWrapper();
        public EventWrapper OnUnfocus = new EventWrapper();
        public string GetText()
        {
            return Loc.Get(LocKey);
        }

        public override void Activate()
        {
            OnUnfocus.Invoke(Entity.Transform);
        //  DoorInteractableHelper.OnDoorActivated(Map, Spawn, position, EffectType);
            foreach (var c in Commands)
                Shell.Exec(c);
            InteractEvent.Invoke(Entity.Transform);
        }

        public override void Deactivate(bool isFocused)
        {
        }

        public override void Focus()
        {
            OnFocus.Invoke(Entity.Transform);
        }

        public override void Unfocus(bool isActivated)
        {
            OnUnfocus.Invoke(Entity.Transform);
        }
    }
}