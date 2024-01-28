// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Input;
using SEQ.Script;
using SEQ.Script.Core;

// TODO wrong namespace, from FPS tutorial
namespace SEQ.Sim
{
    public class PlayerInput : SyncScript
    {
        /// <summary>
        /// Raised every frame with the intended direction of movement from the player.
        /// </summary>
     //   public static readonly EventKey<Vector3> MoveDirectionEventKey = new EventKey<Vector3>();       // This can be made non-static and require specific binding to the scripts instead

        public static readonly EventKey<Vector2> CameraDirectionEventKey = new EventKey<Vector2>();     // This can be made non-static and require specific binding to the scripts instead

      //  public static readonly EventKey<bool> ShootEventKey = new EventKey<bool>();                     // This can be made non-static and require specific binding to the scripts instead

       // public static readonly EventKey<bool> ReloadEventKey = new EventKey<bool>();                    // This can be made non-static and require specific binding to the scripts instead

        public float DeadZone { get; set; } = 0.25f;

        public CameraComponent Camera { get; set; }

        /// <summary>
        /// Multiplies move movement by this amount to apply aim rotati        /// </summary>
        public float MouseSensitivity => SystemPrefsManager.Sensitivity;
        public float StickSensitivity => SystemPrefsManager.StickSensitivity;

       // public List<Keys> KeysLeft { get; } = new List<Keys>();

        //public List<Keys> KeysRight { get; } = new List<Keys>();

 //       public List<Keys> KeysUp { get; } = new List<Keys>();

   //     public List<Keys> KeysDown { get; } = new List<Keys>();

     //   public List<Keys> KeysReload { get; } = new List<Keys>();

        public PlayerInput()
        {
            // Fix single frame input lag
            Priority = -1000;
        }

        public bool IsCancelling()
        {
            return ((
                Keybinds.GetKey(Keybind.Left) && Keybinds.GetKey(Keybind.Right)
                && !Keybinds.GetKey(Keybind.Forward) && !Keybinds.GetKey(Keybind.Backward))
                || (
                !Keybinds.GetKey(Keybind.Left) && !Keybinds.GetKey(Keybind.Right)
                && Keybinds.GetKey(Keybind.Forward) && Keybinds.GetKey(Keybind.Backward))
                ||
                (Keybinds.GetKey(Keybind.Left) && Keybinds.GetKey(Keybind.Right)
                                && Keybinds.GetKey(Keybind.Forward) && Keybinds.GetKey(Keybind.Backward))
                                );
        }

        /*
        bool LeftDown;
        bool RightDown;
        bool ForwardDown;
        bool BackwardDown;
        void UpdateIsDown()
        {

            if (Keybinds.GetKeyDown(Keybind.Left))
                LeftDown = true;
            if (Keybinds.GetKeyDown(Keybind.Right))
                RightDown = true;
            if (Keybinds.GetKeyDown(Keybind.Forward))
                ForwardDown = true;
            if (Keybinds.GetKeyDown(Keybind.Backward))
                BackwardDown = true;

            if (Keybinds.GetKeyUp(Keybind.Left))
                LeftDown = false;
            if (Keybinds.GetKeyUp(Keybind.Right))
                RightDown = false;
            if (Keybinds.GetKeyUp(Keybind.Forward))
                ForwardDown = false;
            if (Keybinds.GetKeyUp(Keybind.Backward))
                BackwardDown = false;
        }*/
        public  Vector3 GetMoveDir()
        {
            // Game controller: left stick
            var moveDirection = Input.GetLeftThumbAny(DeadZone);
            var isDeadZoneLeft = moveDirection.Length() < DeadZone;
            if (isDeadZoneLeft)
                moveDirection = Vector2.Zero;
            else
                moveDirection.Normalize();

            /*
            UpdateIsDown();
            if (LeftDown)
                moveDirection += -Vector2.UnitX;
            if (RightDown)
                moveDirection += +Vector2.UnitX;
            if (ForwardDown)
                moveDirection += +Vector2.UnitY;
            if (BackwardDown)
                moveDirection += -Vector2.UnitY;
            */

            if (Keybinds.S.Input.Keyboard?.Id != LastBoard?.Id)
            {
                Logger.Log(Channel.Input, LogPriority.Info, "Keyboard changed!");
            }
            if (!Keybinds.S.Input.HasKeyboard)
            {

                Logger.Log(Channel.Input, LogPriority.Error, "No board !");
            }
            LastBoard = Keybinds.S.Input.Keyboard;
                // Keyboard
                if (Keybinds.GetKey(Keybind.Left))
                moveDirection += -Vector2.UnitX;
            if (Keybinds.GetKey(Keybind.Right))
                moveDirection += +Vector2.UnitX;
            if (Keybinds.GetKey(Keybind.Forward))
                moveDirection += +Vector2.UnitY;
            if (Keybinds.GetKey(Keybind.Backward))
                moveDirection += -Vector2.UnitY;


            // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
            var worldSpeed = new Vector3(moveDirection.X, 0, moveDirection.Y);
           //     (Camera != null)
             //   ? Utils.LogicDirectionToWorldDirection(moveDirection, Camera, Vector3.UnitY)
              //  : new Vector3(moveDirection.X, 0, moveDirection.Y); // If we don't have the correct camera attached we can send the directions anyway, but they probably won't match

            return worldSpeed.Length() > 1 ? worldSpeed.Normalized : worldSpeed;
        }

        /*
        public Vector3 GetWorldMoveDir()
        {
            // Game controller: left stick
            var moveDirection = Input.GetLeftThumbAny(DeadZone);
            var isDeadZoneLeft = moveDirection.Length() < DeadZone;
            if (isDeadZoneLeft)
                moveDirection = Vector2.Zero;
            else
                moveDirection.Normalize();

            // Keyboard
            if (Keybinds.GetKey(Keybind.Left))
                moveDirection += -Vector2.UnitX;
            if (Keybinds.GetKey(Keybind.Right))
                moveDirection += +Vector2.UnitX;
            if (Keybinds.GetKey(Keybind.Forward))
                moveDirection += +Vector2.UnitY;
            if (Keybinds.GetKey(Keybind.Backward))
                moveDirection += -Vector2.UnitY;

            // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
            var worldSpeed  =
                 (Camera != null)
             ? Utils.LogicDirectionToWorldDirection(moveDirection, Camera, Vector3.UnitY)
            : new Vector3(moveDirection.X, 0, moveDirection.Y); // If we don't have the correct camera attached we can send the directions anyway, but they probably won't match

            return worldSpeed.Length() > 1 ? worldSpeed.Normalized : worldSpeed;
        }
        */
        IInputDevice LastBoard;
        public override  void Update()
        {
            if (GameStateManager.Inst.Active.GetInteractionState() != InteractionState.World)
                return;
            DebugText.Print($"left:{Keybinds.GetKey(Keybind.Left)} right:{Keybinds.GetKey(Keybind.Right)} forward:{Keybinds.GetKey(Keybind.Forward)} back:{Keybinds.GetKey(Keybind.Backward)}", new Int2(100, 180));

            /*
            // Character movement
            //  The character movement can be controlled by a game controller or a keyboard
            //  The character receives input in 3D world space, so that it can be controlled by an AI script as well
            //  For this reason we map the 2D user input to a 3D movement using the current camera
            {
                // Game controller: left stick
                var moveDirection = Input.GetLeftThumbAny(DeadZone);
                var isDeadZoneLeft = moveDirection.Length() < DeadZone;
                if (isDeadZoneLeft)
                    moveDirection = Vector2.Zero;
                else
                    moveDirection.Normalize();

                // Keyboard
                if (KeysLeft.Any(key => Input.IsKeyDown(key)))
                    moveDirection += -Vector2.UnitX;
                if (KeysRight.Any(key => Input.IsKeyDown(key)))
                    moveDirection += +Vector2.UnitX;
                if (KeysUp.Any(key => Input.IsKeyDown(key)))
                    moveDirection += +Vector2.UnitY;
                if (KeysDown.Any(key => Input.IsKeyDown(key)))
                    moveDirection += -Vector2.UnitY;

                // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
                var worldSpeed = (Camera != null)
                    ? Utils.LogicDirectionToWorldDirection(moveDirection, Camera, Vector3.UnitY)
                    : new Vector3(moveDirection.X, 0, moveDirection.Y); // If we don't have the correct camera attached we can send the directions anyway, but they probably won't match

              //  MoveDirectionEventKey.Broadcast(worldSpeed);
            }
            */

            // Camera rotation
            //  Camera rotation is ALWAYS in camera space, so we don't need to account for View or Projection matrices
            {
                // Game controller: right stick
                var cameraDirection = Input.GetRightThumbAny(DeadZone);
                var isDeadZoneRight = cameraDirection.Length() < DeadZone;
                if (isDeadZoneRight)
                    cameraDirection = Vector2.Zero;
                else
                    cameraDirection.Normalize();

                // Contrary to a mouse, driving camera rotation from a stick must be scaled by delta time.
                // The amount of camera rotation with a stick is constant over time based on the tilt of the stick,
                // Whereas mouse driven rotation is already constrained by time, it is driven by the difference in position from last *time* to this *time*.
                if (Input.IsMousePositionLocked)
                    cameraDirection *= (float)this.Game.UpdateTime.Elapsed.TotalSeconds * StickSensitivity;

                // Mouse-based camera rotation.
                //  Only enabled after you click the screen to lock your cursor, pressing escape will cancel it.

                if (Input.IsMousePositionLocked)
                {
                    cameraDirection += new Vector2(Input.MouseDelta.X, -Input.MouseDelta.Y) * SystemPrefsManager.GetTheSens();
                }

                // Broadcast the camera direction directly, as a screen-space Vector2
                CameraDirectionEventKey.Broadcast(cameraDirection);
            }
            /*
            {
                // Controller: Right trigger
                // Mouse: Left button, Tap events
                var didShoot = Input.GetRightTriggerAny(0.2f) > 0.2f;   // This will allow for continuous shooting

                if (Input.PointerEvents.Any(x => x.EventType == PointerEventType.Pressed))
                    didShoot = true;
                    
                if (Input.HasMouse && Input.IsMouseButtonDown(MouseButton.Left))                  // This will allow for continuous shooting
                    didShoot = true;

                ShootEventKey.Broadcast(didShoot);
            }

            {
                // Reload weapon
                var isReloading = Input.IsGamePadButtonDownAny(GamePadButton.X);
                if (KeysReload.Any(key => Input.IsKeyDown(key)))
                    isReloading = true;

                ReloadEventKey.Broadcast(isReloading);
            }*/
        }
    }
}
