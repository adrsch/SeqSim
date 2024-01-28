using BulletSharp;
using Stride.Core.Mathematics;
using Stride.Physics;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public static class Goals
    {
        public static void WalkTo(FactionAI ai, Vector3 p)
        {
            ai.Agent.Navmesh = NavmeshType.Default;
            // Logger.Log(Channel.AI, LogPriority.Trace, $"{ai.DisplayName} entering chase");
            ai.Agent.SetDestination(p);
            ai.IsRunning = false;
        }

        public static void SetNavmeshForChasing(FactionAI ai)
        {
            if (ai.Faction.GetRelation(ai.EffectiveTarget.GetFaction()) == RelationType.Violence)

                ai.Agent.Navmesh = NavmeshType.Smash;
            else
                ai.Agent.Navmesh = NavmeshType.Default;
        }
        public static void ToCalm(FactionAI ai)
        {
            ai.Agent.Navmesh = NavmeshType.Default;
            if (ai.IsRunning)
            {
                ai.Agent.HaltMovement();
                ai.IsRunning = false;
            }
        }

        public static void Wander(FactionAI ai)
        {
            Goals.ToCalm(ai);
            /*
            if (ai.Agent.AtGoal())
            {
                if (Random.Shared.Next(2) == 0)
                {
                    ToPointAround(ai, ai.Transform.WorldPosition);
                }
            }*/
            ai.Agent.IsWandering = true;
        }

        public static void ToPointAround(FactionAI ai, Vector3 point)
        {
            int retry = 0;
            Vector3 offset;
            do
            {
                var verticalOffset = Random.Shared.NextSingle() * retry;
                offset =
                    new Vector3(
                        2 * Random.Shared.NextSingle() - 1,
                        2 * verticalOffset - retry,
                        2 * Random.Shared.NextSingle() - 1
                        );
                retry++;
            } while (!ai.Agent.SetDestination(point + 5 * offset) && retry < 5);
            if (retry == 5)
            {
                Logger.Log(Channel.AI, LogPriority.Info, $"{ai.Actor.State.SeqId}: Can't find a location around target all attempts");
            }
        }

        public static void Shoot(FactionAI ai)
        {
            //ai.FireDown = true;
            ai.CurrentWeapon?.OnFireDown();
        }

        public static void StopShoot(FactionAI ai)
        {
           // ai.FireDown = false;
            ai.CurrentWeapon?.OnFireUp();
        }
        public static void Reload(FactionAI ai)
        {
           // if (ai.CurrentWeapon.AmmoManager.IsReloading)
            //    return;
            //ai.Animator.Reloading = true;
            ai.CurrentWeapon?.Reload();
        }
        static List<HitResult> Hits = new List<HitResult>();
        public static bool IsLineOfSightBlocked(FactionAI ai, ISensorTarget target)
        {
            if (ai.SpottingSensor == null)
                return true;
            var start = ai.SpottingSensor.SensorPosition(true);
            var end = ai.EffectiveTarget.Position;
            Hits.Clear();
            ai.GetSimulation().RaycastPenetrating(start, end, Hits);
            foreach (var hit in Hits)
            {
                if (hit.Succeeded)
                {
                    if (hit.Collider.GetFactionObject() is IPerceptible faction)
                    {
                        if (faction.Faction.CaresAboutKilling(ai.Faction)
                            && ai.ShotBy != faction)
                        {
                            return true;
                        }
                    }
                    else if ((!(hit.Collider is RigidbodyComponent rb && rb.AllowPush))
                        && hit.Collider.SurfaceType != Stride.Engine.SurfaceType.Glass)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsHostile(FactionAI ai, ISensorTarget t)
        {
            var rel = (ai.Faction.GetRelation(t.GetFaction()));
            return rel == RelationType.Violence || ai.ShotBy == t.Perceptible; 
        }
        public static bool IsHostileOrUnsure(FactionAI ai, ISensorTarget t)
        {
            var rel = (ai.Faction.GetRelation(t.GetFaction()));
             return rel == RelationType.Violence || rel == RelationType.Unsure
                || ai.ShotBy == t.Perceptible
                || ai.ShotBy?.Faction == t.GetFaction()
                || (rel == RelationType.Neutral && t.Perceptible.IsArmed);
        }

    }
}
