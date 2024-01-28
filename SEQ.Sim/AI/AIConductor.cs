
using SEQ;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public abstract class AIActivityControllerGeneric<T> : AIActivityControllerBase where T : ActivityLocationBase
    {
        public T Location => LocationBase as T;
    }

    [System.Serializable]
    public abstract class AIActivityControllerBase : IStateController
    {
        public MachineAI AI { get; set; }

        protected StateQueue<AIGoalControllerBase> Goals = new StateQueue<AIGoalControllerBase>();

        public ActivityLocationBase LocationBase;

        public void Enqueue(AIGoalControllerBase goal)
        {
            Logger.Log(Channel.AI, LogPriority.Trace, $"{AI.name}: Enqueuing goal {goal.GetType().Name}");
            goal.AI = AI;
            goal.Activity = this;
            Goals.Enqueue(goal);
        }

        public abstract void OnGainControl();

        public virtual void OnLoseControl()
        {
            Logger.Log(Channel.AI, LogPriority.Trace, $"{AI.name}: clearing goals due to losing control - {this.GetType().Name}");
            Goals.Clear();
            if (LocationBase != null)
                LocationBase.OnLoseControl();
        }

        public virtual void DoUpdate(float dt)
        {
            if (Goals.Active != null)
                Goals.Active.DoUpdate(dt);
        }

        public void OnGoalCompleted()
        {
            Logger.Log(Channel.AI, LogPriority.Trace, $"{AI.name}: finished goal {Goals.Active.GetType().Name}");
            Goals.Dequeue();
            if (Goals.Active == null)
            {
                Logger.Log(Channel.AI, LogPriority.Trace, $"{AI.name}: all goals completed");
                OnGoalsCompleted();
            }
            else
                Logger.Log(Channel.AI, LogPriority.Trace, $"{AI.name}: next goal {Goals.Active.GetType().Name}");
        }

        protected abstract void OnGoalsCompleted();
    }

    [System.Serializable]
    public abstract class AIGoalControllerBase : IStateController
    {
        public AIActivityControllerBase Activity;
        public MachineAI AI { get; set; }

        public abstract void OnGainControl();

        public abstract void OnLoseControl();

        public void DoUpdate(float dt)
        {
            Update(dt);
            if (IsCompleted())
            {
                if (Activity != null)
                    Activity.OnGoalCompleted();
            }
        }

        protected abstract void Update(float dt);

        protected abstract bool IsCompleted();
    }

    public class SendCallbackGoal : AIGoalControllerBase
    {
        public Action Callback;
        public SendCallbackGoal(Action cb) => Callback = cb;
        public override void OnGainControl()
        {
            Callback?.Invoke();
        }

        public override void OnLoseControl()
        {
        }

        protected override bool IsCompleted()
        {
            return true;
        }

        protected override void Update(float dt)
        {
        }
    }

    [System.Serializable]
    public class NavigateToPointGoal : AIGoalControllerBase
    {
        public Vector3 Destination;
        public bool UseApprox;
        public bool UseRun;
        public NavigateToPointGoal(Vector3 pos, bool approx = false)
        {
            Destination = pos;
            UseApprox = approx;
        }

        public void SetNewDestination(Vector3 pos)
        {
            Destination = pos;
            AI.SetGoal(Destination);
        }


        public override void OnGainControl()
        {
            AI.UseRun = UseRun;
            AI.SetGoal(Destination);
        }

        public override void OnLoseControl()
        {
            AI.UseRun = false;
        }

        protected override bool IsCompleted()
        {
            var atDest = AI.Agent.AtGoal();
            return atDest || (UseApprox && Vector3.Distance(new Vector3(Destination.x, 0, Destination.z), new Vector3(AI.Transform.WorldPosition.x, 0, AI.Transform.WorldPosition.z)) < 1.33f);
        }

        protected override void Update(float dt)
        {
            AI.UseRun = UseRun;
        }
    }


    [System.Serializable]
    public class ChaseGoal : AIGoalControllerBase
    {
        public TransformComponent Target;
        public bool UseRun;
        public ChaseGoal(TransformComponent target)
        {
            Target = target;
        }

        public override void OnGainControl()
        {
            AI.UseRun = UseRun;
            AI.SetGoal(Target.WorldPosition);
        }

        public override void OnLoseControl()
        {
            AI.UseRun = false;
        }

        protected override bool IsCompleted()
        {
            var atDest = AI.HasReachedDestination(Target.WorldPosition);
            return atDest;
        }

        protected override void Update(float dt)
        {
            AI.UseRun = UseRun;
            AI.Agent.SetDestination(Target.WorldPosition);
        }
    }

    [System.Serializable]
    public class TeleportGoal : AIGoalControllerBase
    {
        public Vector3 Destination;
        public Quaternion Rotation;
        public TeleportGoal(Vector3 pos, Quaternion rot)
        {
            Destination = pos;
            Rotation = rot;
        }


        public override void OnGainControl()
        {
            AI.SetPositionAndRotation(Destination, Rotation);
        }

        public override void OnLoseControl()
        {
        }

        protected override bool IsCompleted()
        {
            return true;
        }

        protected override void Update(float dt)
        {
        }
    }

    //  public class ShootAtTargetGoal : AIGoalControllerBase
    //{
    //   public 
    //}

    public class WaitGoal : AIGoalControllerBase
    {
        public float Duration;
        public WaitGoal(float d) => Duration = d;

        float FinishTime;
        public override void OnGainControl()
        {
            FinishTime = Time.time + Duration;
        }

        public override void OnLoseControl()
        {
        }

        protected override bool IsCompleted()
        {
            if (Time.time >= FinishTime)
                return true;
            return false;
        }

        protected override void Update(float dt)
        {
        }
    }
    public class WaitUntilSignalGoal : AIGoalControllerBase
    {
        public bool IsComplete;

        public override void OnGainControl()
        {
            IsComplete = false;
        }

        public override void OnLoseControl()
        {
        }

        protected override bool IsCompleted()
        {
            if (IsComplete)
                return true;
            return false;
        }

        protected override void Update(float dt)
        {
        }
    }

    public class FaceDirectionGoal : AIGoalControllerBase
    {
        public Quaternion TargetRotation;
        public FaceDirectionGoal(Quaternion f) => TargetRotation = f;
        public override void OnGainControl()
        {
        }

        public override void OnLoseControl()
        {
        }


        protected override bool IsCompleted()
        {
            if (Quaternion.AngleBetween(AI.Transform.Rotation, TargetRotation) < 0.1f)
            {
                AI.Transform.Rotation = TargetRotation;
                AI.SetPositionAndRotation(AI.Transform.WorldPosition, AI.Transform.Rotation);
                return true;
            }
            return false;
        }

        protected override void Update(float dt)
        {
            AI.Transform.Rotation = Quaternion.RotateTowards(AI.Transform.Rotation, TargetRotation, AI.TurnSpeed * dt);
            AI.SetPositionAndRotation(AI.Transform.WorldPosition, AI.Transform.Rotation);
            AI.Animator.DoMoveUpdate(new CharacterMoveUpdate
            {
                Height = 0f,
                IsGrounded = true,
                Jumped = false,
                WorldForward = AI.Transform.Forward,
                Velocity = Vector3.zero,
            });
        }
    }


    public class PlayAnimationGoal : AIGoalControllerBase
    {
        public string AnimationTrigger;
        public string ExpectedMessage;
        // trigger suint a trigger its jsjt an event
        public PlayAnimationGoal(string trigger, string msg = null)
        {
            AnimationTrigger = trigger;
            ExpectedMessage = msg;
        }
        public override void OnGainControl()
        {
            if (ExpectedMessage != null)
                AI.Animator.OnAnimationEvent += OnAnimationEvent;
            AI.Animator.SetTrigger(AnimationTrigger);
        }

        public override void OnLoseControl()
        {
            if (ExpectedMessage != null)
                AI.Animator.OnAnimationEvent -= OnAnimationEvent;
        }

        protected override bool IsCompleted() => ExpectedMessage == null || hasReceivedEvent;

        bool hasReceivedEvent;
        void OnAnimationEvent(string message)
        {
            if (message == ExpectedMessage)
                hasReceivedEvent = true;
        }

        protected override void Update(float dt)
        {
        }
    }

    public struct ActivityRegistery
    {
        public ActivityType Activity;
        public ActivityLocationBase Location;
    }


    public enum InfractionLevel
    {
        None,
        Minor,
        Major,
        Extreme,
    }
    public interface IMoveNavAgent
    {
        void MoveNavAgent(Vector3 position, Quaternion rotation);
    }

}