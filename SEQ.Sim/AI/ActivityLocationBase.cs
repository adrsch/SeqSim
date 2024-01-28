using SEQ;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum ActivityType
    {
        Wander = 0,
    }
    public abstract class ActivityLocationBase : ScriptComponent
    {
        public abstract ActivityType GetActivityType();

        protected abstract AIActivityControllerBase GetController();
        public bool IsBound;
        public AIActivityControllerBase BeginActivity(MachineAI machine)
        {
            IsBound = true;

            var controller = GetController();
        //    BoundController = controller;
            controller.LocationBase = this;
            machine.Push(controller);
            return controller;
        }

        protected abstract int GetPriorityForMachine(MachineAI machine);
        public void OnLoseControl()
        {

        }
    }
}