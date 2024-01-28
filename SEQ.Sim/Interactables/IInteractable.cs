using BulletSharp;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum InteractType
    {
        Focus,
        Activate,
        Deactivate,
        Unfocus,
    }
    public class InteractEvent
    {
        public IInteractable Target;
        public InteractType Type;
    }

    public enum InteractableDistance
    {
        Default = 0,
        Item = 1,
        Small = 2,
        Big = 3,
        Disabled = 4,
    }
    public interface IInteractable
    {
        void Focus();
        void Unfocus(bool isActivated);
        void Activate();
        void Deactivate(bool isFocused);
        InteractableDistance DistanceClass { get; }
    }

    public interface IFocusText
    {
        string GetText();
    }

}