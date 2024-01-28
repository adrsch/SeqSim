using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class MachineAI : StartupScript
    {
        public NavMeshAgent Agent;
        [DataMemberIgnore]
        public TransformComponent TargetTransform;
        public Actor Actor;
        public SimDamageable Damageable;

        public string name => Actor.Entity.Name;

        public override void Start()
        {
            if (Actor != null)
            {
                Actor.OnPositionChangedAction += (p, r) =>
                {
                    ForcePositionAndRotation(p, r);
                };

                Actor.ActiveUpdate += DoUpdate;
            }
        }
        [DataMemberIgnore]
        public StateStack<AIActivityControllerBase> Activities = new StateStack<AIActivityControllerBase>();
        public void Push(AIActivityControllerBase state)
        {
            state.AI = this;
            Activities.Push(state);
        }

        public AnimatorUpdaterBase Animator;
        bool IsNavActive;

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Transform.WorldPosition = position;
            Transform.Rotation = rotation;
            Agent.Warp(position);
        }

        public void ForcePositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Transform.WorldPosition = position;
            Transform.Rotation = rotation;
            Agent.UseNavigation = false;
            Agent.Warp(position);
            Transform.WorldPosition = position;
            IsNavActive = false;

        }

        public float TurnSpeed;
        public void SetGoal(Vector3 pos)
        {
            Agent.UseNavigation = true;
            IsNavActive = true;
            Logger.Log(Channel.AI, LogPriority.Trace, $"{name}: setting nav dest {pos}");
            if (!IsNavActive)
                Agent.Warp(Transform.WorldPosition);
            Agent.SetDestination(pos);
        }

    //    public float TurnSpeed => Agent.angularSpeed;

        public bool HasReachedDestination(Vector3 pos)//, bool useRemainingDistance = false)
        {
            return Agent.AtDestination(pos);
            /*
            if (Vector3.Distance(pos, Transform.WorldPosition) < DistanceSnap && Agent.velocity.magnitude < VelocitySnap)
            {
                Logger.Log(Channel.AI, LogPriority.Debug, $"{name}:  nav destination reached");
                Agent.isStopped = true;
                IsNavActive = false;
                Transform.WorldPosition = pos;
                DoAnimUpdate(Vector3.zero);
                return true;
            }
            return false;
            */
        }

        void UpdateActivity(float dt)
        {
            if (Activities.Active != null)
                Activities.Active.DoUpdate(dt);
        }
        public static bool DisableAI;

        private void DoUpdate(float dt)
        {
            if (Damageable != null && Damageable.IsDead)
                return;
            if (!DisableAI)
            {
                UpdateActivity(dt);
                if (TargetTransform != null)
                {
                    Agent.SetDestination(TargetTransform.WorldPosition);
                }
                UpdateNav(dt);
            }
        }

        void UpdateNav(float dt)
        {
            if (IsNavActive)
            {
                if (Agent.UseNavigation)
                {
                    DoAnimUpdate(dt, Agent.Velocity);
                }
            }
        }

        public bool UseRun;
       public float WalkSpeed;
      public float SprintSpeed;

        void DoAnimUpdate(float dt, Vector3 vel)
        {
       //     if (vel.Magnitude > 0)
         //       Transform.Rotation = Quaternion.RotateTowards(Transform.Rotation, Quaternion.LookRotation(vel.Normalized, Vector3.up), TurnSpeed * dt);

            if (Animator != null)
                Animator.DoMoveUpdate(new CharacterMoveUpdate
                {
                    Height = 0f,
                    IsGrounded = true,
                    Jumped = false,
                    WorldForward = Transform.Forward,
                    Velocity = vel,///vel.magnitude > 0.5f ? vel : Vector3.zero,
                    IsRunning = UseRun,
                });


        }
    }
}