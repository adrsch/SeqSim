using BulletSharp;
using Stride.Core;
using Stride.Engine;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{

    public abstract class InteractableBase : StartupScript, IInteractable
    {
        public virtual void Activate()
        {

        }
        public virtual void Deactivate(bool isFocused)
        {

        }
        public virtual void Focus()
        {

        }
        public virtual void Unfocus(bool isActivated)
        {

        }

        public InteractableDistance BaseDistanceClass = InteractableDistance.Default;

        public bool IsInteractionDisabled;

        public virtual InteractableDistance DistanceClass => IsInteractionDisabled ? InteractableDistance.Disabled : BaseDistanceClass;

        public void EnableInteraction()
        {
            IsInteractionDisabled = false;
        }

        public void DisableInteraction()
        {
            IsInteractionDisabled = true;
        }

    }
}