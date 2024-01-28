
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public interface IAnimatorUpdater
    {
        void DoMoveUpdate(CharacterMoveUpdate update);
        void SetBool(string name, bool val);
        void SetTrigger(string trigger);
        void SetInt(string name, int val);
        void Warp();
    }
    public abstract class AnimatorUpdaterBase : StartupScript, IAnimatorUpdater
    {
        public Action<string> OnAnimationEvent;

        public abstract void DoMoveUpdate(CharacterMoveUpdate update);
        public abstract void SetBool(string name, bool val);
        public abstract void SetInt(string name, int val);
        public abstract void SetTrigger(string trigger);
        public abstract void Warp();

        public void AnimationEvent(string message)
        {
            OnAnimationEvent?.Invoke(message);
        }

    }
}