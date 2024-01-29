using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public interface ISolveSpeed
    {
        float GetSpeed();
    }
    public class NavMeshAgent : StartupScript
    {
        //public NavigationComponent Nav;
        public RigidbodyComponent Rb;

        [DataMemberIgnore]
        public bool UseNavigation = true;

        public bool AtGoal()
        {
            return (Index >= Path.Count);
        }

        public ISolveSpeed SpeedSolver;

        private Vector3 CurrentWaypoint => Index < Path.Count ? Path[Index] : Vector3.Zero;

        public void UpdateMovement(float dt, IPerceptible p)
        {
            if (Velocity.Magnitude > 0 && MathUtil.IsZero(Vector3.Distance(LastPosition, Position)))
            {
                Position = Position + 2 * Velocity * dt;
                Logger.Log(Channel.Gameplay, LogPriority.Warning, $"Stuck detected for NavAgent {Entity.Name}. Attempting unstuck...");
            }
            else
            {
                var pushaway = Pushaway.Process(p);
                if (pushaway.Magnitude > 0)
                {
                    Position = Position + 2 * pushaway * dt;
                }
            }


            LastPosition = Entity.Transform.WorldMatrix.TranslationVector;

            if (IsWandering)
            {
                var testVelocity = SpeedSolver.GetSpeed().ToXenko() * WanderDirection;
                var from = Transform.WorldPosition;
                var to = Transform.WorldPosition + testVelocity* dt;
                var hit = //Rb.Simulation.ShapeSweep(Rb.ColliderShape, from, to);
                    Rb.Simulation.Raycast(from + Vector3.up, to + Vector3.up);
                if (!hit.Succeeded)
                {
                    var snapHit = (Rb.Simulation.Raycast(to + Vector3.up * 2, to - Vector3.up * 4));

                    if (snapHit.Succeeded)
                    {
                        FallVelocity = 0f;
                        Position = new Vector3(to.x, snapHit.Point.y, to.z);
                    }
                    else
                    {
                        FallVelocity += PhysicsConstants.Gravity.ToXenko() * dt;

                        Position = new Vector3(to.x, from.y - FallVelocity * dt, to.z);
                        // NewDirection();
                    }

                }
                else
                {
                    NewDirection();
                }

                Transform.Rotation = Quaternion.LookRotation(WanderDirection, Vector3.up);
              //  SnapToGround(moved, dt);
            }
            else
            {
                UpdateMoveTowardsDestination();

                if (Velocity.Magnitude > 0)
                {
                    SnapToGround(Position + Velocity * dt, dt);
                }
            }

        }

        float FallVelocity;

        void SnapToGround(Vector3 moved, float dt)
        {
            var hit = (Rb.Simulation.Raycast(Transform.WorldPosition + Vector3.up * 2, Transform.WorldPosition - Vector3.up * 4));
            if (hit.Succeeded)
            {
                FallVelocity = 0f;
                moved = new Vector3(moved.x, hit.Point.y, moved.z);
            }
            else
            {
                FallVelocity += PhysicsConstants.Gravity.ToXenko() * dt;
                moved = new Vector3(moved.x, hit.Point.y - FallVelocity * dt, moved.z);
            }
            Position = moved;
        }

        Vector3 WanderDirection;

        void NewDirection()
        {
            var forward = new Vector3(2 * Random.Shared.NextSingle() - 1, 0, 2 * Random.Shared.NextSingle() - 1);
            WanderDirection = forward;
        }


        public void OrientToSurfaceNormal()
        {
            var hit = (Rb.Simulation.Raycast(Transform.WorldPosition + Vector3.up * 1, Transform.WorldPosition - Vector3.up * 1));
            if (hit.Succeeded)
            {
                Transform.Rotation = Quaternion.LookRotation(Transform.Forward, hit.Normal);
            }
        }
        public void OrientToWorldUp()
        {
            Transform.Rotation = Quaternion.LookRotation(Transform.Forward, Vector3.up);
        }

        /// <summary>
        /// The distance from the destination at which the character will stop moving
        /// </summary>
        public float DestinationThreshold { get; set; } = 1f;

        public bool AtDestination(Vector3 p)
        {
            return AtGoal() && Vector3.Distance(p, Entity.Transform.WorldMatrix.TranslationVector) < DestinationThreshold;
        }

        /// <summary>
        /// A number from 0 to 1 indicating how much a character should slow down when going around corners
        /// </summary>
        /// <remarks>0 is no slowdown and 1 is completely stopping (on >90 degree angles)</remarks>
        public float CornerSlowdown { get; set; } = 0.6f;
        /// <summary>
        /// Multiplied by the distance to the target and clamped to 1 and used to slow down when nearing the destination
        /// </summary>
        public float DestinationSlowdown { get; set; } = 0.4f;

        Vector3 LastPosition;
        private void UpdateMoveTowardsDestination()
        {
            if (!AtGoal())
            {
                var direction = CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector;

                // Get distance towards next point and normalize the direction at the same time
                var length = direction.Length();
                direction /= length;

                // Check when to advance to the next waypoint
                bool advance = false;

                // Check to see if an intermediate point was passed by projecting the position along the path
                if (Path.Count > 0 && Index > 0 && Index != Path.Count - 1)
                {
                    Vector3 pointNormal = CurrentWaypoint - Path[Index - 1];
                    pointNormal.Normalize();
                    float current = Vector3.Dot(Entity.Transform.WorldMatrix.TranslationVector, pointNormal);
                    float target = Vector3.Dot(CurrentWaypoint, pointNormal);
                    if (current > target)
                    {
                        advance = true;
                    }
                }
                else
                {
                    if (length < DestinationThreshold) // Check distance to final point
                    {
                        advance = true;
                    }
                }

                // Advance waypoint?
                if (advance)
                {
                    Index++;
                    if (AtGoal())
                    {
                        // Final waypoint reached
                        HaltMovement();
                        return;
                    }
                }

                // Calculate speed based on distance from final destination
                float moveSpeed = (Target - Entity.Transform.WorldMatrix.TranslationVector).Length() * DestinationSlowdown;
                if (moveSpeed > 1.0f)
                    moveSpeed = 1.0f;

                // Slow down around corners
                float cornerSpeedMultiply = Math.Max(0.0f, Vector3.Dot(direction, moveDirection)) * CornerSlowdown + (1.0f - CornerSlowdown);

                // Allow a very simple inertia to the character to make animation transitions more fluid
                moveDirection = moveDirection * 0.85f + direction * moveSpeed * cornerSpeedMultiply * 0.15f;

//                DebugText.Print($"{Velocity}", new Int2(75, 75));
                // character.SetVelocity(moveDirection * speed);
                Velocity = moveDirection * SpeedSolver.GetSpeed().ToXenko();

                // // Broadcast speed as per cent of the max speed
                //  RunSpeedEventKey.Broadcast(moveDirection.Length());

                // Character orientation

                if (AlwaysFaceTarget)
                {
                    var flatFace = new Vector3(FaceTarget.x, 0, FaceTarget.z);
                    var flatCurrent = new Vector3(Transform.WorldPosition.x, 0, Transform.WorldPosition.z);
                    var flatDelta = (flatFace - flatCurrent);


                    yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-flatDelta.Z, flatDelta.X) + MathUtil.PiOverTwo);

                    Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);

                }
                else
                {
                    if (moveDirection.Length() > 0.001)
                    {
                        yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-moveDirection.Z, moveDirection.X) + MathUtil.PiOverTwo);
                    }
                    Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
                }
            }
            else
            {
                // No target
                HaltMovement();
            }
            /*
            if (Velocity == Vector3.Zero)
            {

                var flatFace = new Vector3(FaceTarget.x, 0, FaceTarget.z);
                var flatCurrent = new Vector3(Transform.WorldPosition.x, 0, Transform.WorldPosition.z);
                var flatDelta = (flatFace - flatCurrent);


                yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-flatDelta.Z, flatDelta.X) + MathUtil.PiOverTwo);
                Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);

            }*/
        }
        private float yawOrientation;

        public void HaltMovement()
        {
            moveDirection = Vector3.Zero;
            Velocity = Vector3.Zero;
            //  character.SetVelocity(Vector3.Zero);
            Target = Transform.WorldMatrix.TranslationVector;
            IsWandering = false;
            WanderDirection = Transform.Forward;
        }

        public void Warp(Vector3 position)
        {
            Transform.WorldPosition = position;
            Rb?.UpdatePhysicsTransformation();
        }
        Vector3 moveDirection;
        [DataMemberIgnore]
        public Vector3 Velocity;

        bool AlwaysFaceTarget;
        Vector3 FaceTarget;
        public bool IsFacing(Vector3 p)
        {
            return Transform.IsFacing(p);
        }

        Vector3 Position
        {
            get { return Transform.WorldPosition; }
            set
            {
                Transform.WorldPosition = value;

                Rb?.UpdatePhysicsTransformation();
            }
        }
        Vector3 Target;
        List<Vector3> Path = new List<Vector3>();
        int Index;
        /*
        public void Recalculate()
        {

        //    var downcast = Nav.Raycast(Target, Target + Vector3.down * 5f, QuerySettings);
         //   if (downcast.Hit)
           // {
                var newPath = new List<Vector3>();
                if (Nav.TryFindPath(Target, newPath, QuerySettings))
                {
                    Index = 0;
                    Path = newPath;
                }
                else
                {
                    Logger.Log(Channel.AI, LogPriority.Warning, $"Can't find path for {Entity.Name}");
                }
      //      }
        //    else
         //   {
           //     Logger.Log(Channel.AI, LogPriority.Warning, $"Can't raycast onto navmesh for {Entity.Name}");
//
          //  }
        }*/
        public NavmeshType Navmesh;

        public bool IsWandering;

        public bool SetDestination(Vector3 destination, bool facetarg = false)
        {
            IsWandering = false;
            WanderDirection = Transform.Forward;
            // Navmeshes.S.DefaultNav.NavigationMesh.Layers.Values.First().FindTile
            AlwaysFaceTarget = facetarg;
            FaceTarget = destination;

            Vector3 delta = Target - destination;
            if (delta.Length() > 1f) // Only recalculate path when the target position is different
            {
                // Generate a new path using the navigation component
                //Path.Clear();
                var newPath = new List<Vector3>();
               // if (Nav.TryFindPath(destination, Path))
                if (Navmeshes.S.TryFindPath(Transform.WorldPosition, destination, Navmesh, newPath))
                {
                    // Skip the points that are too close to the player
                    Path = newPath;
                    Index = 0;
                    while (!AtGoal() && (CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector).Length() < 0.25f)
                    {
                        Index++;
                    }

                    // If this path still contains more points, set the player to running
                    if (!AtGoal())
                    {
                        Target = destination;
                    }

                    return true;
                }
                else
                {
                    // Could not find a path to the target location
                    //Path.Clear();
                        Logger.Log(Channel.AI, LogPriority.Trace, $"Can't find path for {Entity.Name}");

                    // HaltMovement();
                    IsWandering = true;
                    return false;
                }
            }
            return true;
        }
    }
}
