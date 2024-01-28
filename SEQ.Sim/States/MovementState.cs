using System;
using System.Collections;
using System.Collections.Generic;
using Stride.Engine;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

// TODO cleanup

namespace SEQ.Sim
{
    public class MovementState : IGameStateController
    {
        public static MovementState Inst = new MovementState();
        public InteractionState GetInteractionState()
        {
            // return !ClickMove.Active.DisableTemp ? InteractionState.Standard : InteractionState.GUI | InteractionState.Overlay;
            return InteractionState.World;
        }

        public PointerState GetPointerState()
        {
            //  return PointerState.Free;
            return IsFP() ? PointerState.Locked : PointerState.Free;
        }

        public void OnGainControl()
        {
            //   OrbitCam.Inst.UpdatePriority();
            SetView();
         //   PlayerMovementManager.Inst.EnableMovement();
        }

        public void OnLoseControl()
        {
         //   PlayerMovementManager.Inst.DisableMovement();
            // OrbitCam.Inst.FreeLook.m_XAxis.Value = PlayerMovementManager.Inst.ThirdPersonCamera.FreeLook.m_XAxis.Value;
        }

        void SetView()
        {
            if (false)
            {
                SetToThird();
            }
            else
            {
                SetToFirst();
            }
        }

        public bool IsFP()
        {
            return true;
    //        return !Player.Data.UseThirdPerson;
        }

        void SetToThird()
        {
         //   PlayerMovementManager.Inst.SetToThirdPerson();
        }

        void SetToFirst()
        {
         //   PlayerMovementManager.Inst.SetToFirstPerson();
        }
        /*

        void AddCharacterHeadToFirstPersonCamera()
        {
           PlayerMovementManager.Inst.FirstPersonCamera.cullingMask |= (1 << (int)MaskLayers.CharacterHead);
        }

        void RemoveCharacterHeadFromFirstPersonCamera()
        {
           PlayerMovementManager.Inst.FirstPersonCamera.cullingMask &= (~(1 << (int)MaskLayers.CharacterHead));
        }*/
        public static void PushMovementState()
        {
            GameStateManager.Push(Inst);
        }
        /*
        [Command("Set controller to pilot mode")]
        public static void ToPilotMode()
        {
            Inst.ToPilotModeInternal();
        }

        void ToPilotModeInternal()
        {
            RemoveCharacterHeadFromFirstPersonCamera();
            PilotManager.Inst.EnablePilotMode();
            EventManager.Raise(new EnterMechaEvent { InMecha = false });
        }

        [Command("Set controller to mecha mode")]
        public static void ToMechaMode()
        {
            Inst.ToMechaModeInternal();
        }

        void ToMechaModeInternal()
        {
            AddCharacterHeadToFirstPersonCamera();
            EventManager.Raise(new EnterMechaEvent { InMecha = true });
        }*/
    }
}