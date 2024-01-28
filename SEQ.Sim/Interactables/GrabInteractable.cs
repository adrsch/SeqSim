
using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{

    public class GrabInteractable : SyncScript, IInteractable, IFocusText
    {
        public string LocKey;
        public List<string> Commands = new List<string>();
        public EventWrapper InteractEvent = new EventWrapper();

        
        public EventWrapper OnFocus = new EventWrapper();
        public EventWrapper OnUnfocus = new EventWrapper();

        public InteractableDistance DistanceClass { get; set; }

        public string GetText()
        {
            return Loc.Get(LocKey);
        }
        public  void Activate()
        {
            OnUnfocus.Invoke(Entity.Transform);
            InteractionProbe.S.OverrieActivate(() =>
            {
                IsGrabbing = true;
                Drop();
            });
        //  DoorInteractableHelper.OnDoorActivated(Map, Spawn, position, EffectType);
            foreach (var c in Commands)
                Shell.Exec(c);
            InteractEvent.Invoke(Entity.Transform);
        }

        bool IsGrabbing;
        void Drop()
        {
        }

        void Throw()
        {
        }

        public  void Deactivate(bool isFocused)
        {
        }

        public  void Focus()
        {
            OnFocus.Invoke(Entity.Transform);
        }

        public  void Unfocus(bool isActivated)
        {
            OnUnfocus.Invoke(Entity.Transform);
        }

        public RigidbodyComponent Rb;
        public override void Update()
        {
            if (IsGrabbing)
            {
              //  Rb.
            }
        }
    }
}