using SEQ.Script;
using Stride.Input;
using Stride.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine.Processors;

using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class PlayerWeaponManager : AsyncCvarMultiListenerBase
    {
        public static PlayerWeaponManager S;

        public override async Task BeforeInit()
        {
            S = this;
            while (Player == null)
            {
                await Script.NextFrame();
            }
            SelectedSlot = new InventorySlot(HUD.Sim.UI.Page.RootElement.FindVisualChildOfType<UIElement>("selectedslot"), "player");
            SelectedSlot.ClearSelected();
            SelectedSlot.Clear();
            Inventory = Entity.Get<InventoryDisplay>();

        }
        public InventoryDisplay Inventory;
        public InventorySlot SelectedSlot;
        ActorState CurrentEquipped => Inventory.Slots[Equipped].BoundState;

        ActorUsable EquippedUsable;

        ActorState Player => PlayerData.State;
        /*
        ActorUsable GetCurrentUsable()
        {
            var equip = CurrentEquipped;
            if (equip != null)
            {
                return ActorUsable.Get(equip.Species);
            }
            return null;
        }
        ActorUsable GetUsable(int slot)
        {
            var equip = Player.GetChild(slot);
            if (equip != null)
            {
                return ActorUsable.Get(equip.Species);
            }
            return null;
        }*/

        ActorState GetEntity(int slot)
        {
            return Player.GetChild(slot);
        }

        int Equipped;

        const float UnequipTime = 0.5f;
        const float EquipTime = 0.5f;

        bool Unequipping;
        bool Equipping;


        public void DisableEquipped()
        {

        }

        public void EnableEquipped()
        {

        }

        public void Refresh()
        {
            var equippedState = Player.GetChild(EquippedSlot);
            if (EquippedUsable != null)
            {
                if (EquippedUsable.Bound != equippedState)
                {
                    if (!Unequipping)
                    {
                        OnUnequippedAction = () => DoEquip(EquippedSlot);
                        //  StartCoroutine(UnequipRoutine());
                    }
                }
            }
            else
            {
                if (equippedState != null)
                {

                    DoEquip(EquippedSlot);
                }
            }

            /*
            if (equippedUsable != EquippedUsable)
            {
                if ((EquippedUsable != null && equippedUsable == null)
                    )
                {
                }
                else if (equippedUsable != null)
                {
                    if (EquippedUsable != null)
                    {
                        if (!Unequipping)
                        {
                            OnUnequippedAction = () => DoEquip(EquippedSlot);
                          //  StartCoroutine(UnequipRoutine());
                        }

                    }
                    else
                    {
                        DoEquip(EquippedSlot);
                    }

                }
            }
            if (EquippedUsable != null && EquippedUsable.Bound != null && EquippedUsable.Bound.Quantity <= 0)
            {
                if (!Unequipping)
                {
                    OnUnequippedAction = () => DoEquip(EquippedSlot);
                //    StartCoroutine(UnequipRoutine());
                }
            }
            if (EquippedUsable != null && EquippedUsable.Bound != null && EquippedUsable.Bound.Parent != "player")
            {
                if (!Unequipping)
                {
                    OnUnequippedAction = () => DoEquip(EquippedSlot);
                    //StartCoroutine(UnequipRoutine());
                }
            }
            */
            if (EquippedUsable != null)
                SelectedSlot.Bind(EquippedUsable.Bound);
            else
                SelectedSlot.Bind(null);
            //   Inventory.Slots[Equipped].Bind(Inventory.Slots[Equipped].BoundState);
            //  if 
        }

        public static void EquipWeapon(int slot)
        {
            if (S.Inventory.Slots.Count > slot)
            {
                Cvars.Set("equipped", slot.ToString());
            }
            else
            {
               // Logger.Log(Channel.Gameplay, Priority.Error, $"Could not get weapon slot {slot}");
            }
        }
        public static void ReloadWeapon()
        {
            if (S.Unequipping || S.Equipping)
                return;

            var thething = S.EquippedUsable;
            if (thething != null)
                thething.Reload();
        }
        System.Action OnUnequippedAction;

        void TryEquip(int slot)
        {
            foreach (var s in Inventory.Slots)
                s.ClearSelected();

            Inventory.Slots[slot].SetAsSelected();

            if (Equipped == slot)
                return;

            G.S.Script.AddTask(async () => await EquipAsync(slot));
        }

        async Task EquipAsync(int slot)
        {
            if (Equipped == slot)
            {
                return;
            }
            SelectedSlot.Bind(Inventory.Slots[slot].BoundState);
            Equipped = slot;
            OnUnequippedAction = () => DoEquip(slot);
            if (!Unequipping && GetEntity(EquippedSlot) == null)
            {
                OnUnequippedAction.Invoke();
                OnUnequippedAction = null;
            }
            else
            {
                if (!Unequipping)
                {
                    await UnequipRoutine();
                }
            }
        }

        async Task EquipRoutine()
        {
            var thething = EquippedUsable;
            if (thething != null)
            {
                thething.Equip();
                Equipping = true;
                var completeTime = Time.time + EquipTime;
                var startTime = Time.time;
                while (completeTime > Time.time)
                {
                    PlayerCamera.Sim.FP.Transform.RotationEulerXYZ = new Vector3(

                        MathUtil.Deg2Rad * MathUtil.SmootherStep(/*90 -*/ (1 - (Time.time - startTime) / EquipTime)) * 30, 0, 0);
                    await Script.NextFrame();
                }
                PlayerCamera.Sim.FP.Transform.Rotation = Quaternion.Identity;
            }
            Equipping = false;
            if (thething != null)
            {
                thething.EquipFinished();

                PlayerAnimator.S.IsUnequipping = false;
            }
        }
        int EquippedSlot;


        async Task UnequipRoutine()
        {
            if (EquippedUsable != null)
            {
                EquippedUsable.Unequip();
                Unequipping = true;
                PlayerAnimator.S.IsUnequipping = true;
                var completeTime = Time.time + UnequipTime;
                var startTime = Time.time;
                while (completeTime > Time.time)
                {
                    PlayerCamera.Sim.FP.Transform.RotationEulerXYZ = new Vector3(
                    MathUtil.Deg2Rad * MathUtil.SmootherStep((Time.time - startTime) / UnequipTime) * 30, 0, 0);
                    await Script.NextFrame();
                }

                // TODO: sketch
                EquippedUsable.FinishedUnequip();
            }
            Unequipping = false;
            OnUnequippedAction?.Invoke();
            OnUnequippedAction = null;
        }
        
        void DoEquip(int slot)
        {
            var old = EquippedUsable;
            var next = ActorUsable.Get(CurrentEquipped, PlayerAnimator.S);
            EquippedSlot = slot;
            if (old != null)
                old.FinishedUnequip();

            EquippedUsable = next;
            if (next != null)
                next.Equip();
            G.S.Script.AddTask(async () => await EquipRoutine());
        }
        public void OnUnequipped()
        {
            if (OnUnequippedAction != null)
            {
                OnUnequippedAction.Invoke();
                OnUnequippedAction = null;
            }
        }

        const float mwheelcooldown = 0.055f;
        float lastScrollWheeledTime;
        bool shootDown;
        public override async Task AfterInit()
        {
            while (true)
            {
                /*
                if (Player == null)
                {
                    while (Player == null)
                    {
                        await Script.NextFrame();
                    }

                }*/
                if (GameStateManager.Inst.Active is MovementState ms)
                {
                    if (Keybinds.GetKeyDown(Keybind.Reload))
                    {
                        ReloadWeapon();
                    }
                    if (Keybinds.GetKeyDown(Keybind.Slot1)) EquipWeapon(0);
                    if (Keybinds.GetKeyDown(Keybind.Slot2)) EquipWeapon(1);
                    if (Keybinds.GetKeyDown(Keybind.Slot3)) EquipWeapon(2);
                    if (Keybinds.GetKeyDown(Keybind.Slot4)) EquipWeapon(3);
                    if (Keybinds.GetKeyDown(Keybind.Slot5)) EquipWeapon(4);
                    if (Keybinds.GetKeyDown(Keybind.Slot6)) EquipWeapon(5);
                    if (Keybinds.GetKeyDown(Keybind.Slot7)) EquipWeapon(6);
                    if (Keybinds.GetKeyDown(Keybind.Slot8)) EquipWeapon(7);
                    if (Keybinds.GetKeyDown(Keybind.Slot9)) EquipWeapon(8);
                    if (Keybinds.GetKeyDown(Keybind.Slot10)) EquipWeapon(9);
                    if (G.S.Input.MouseWheelDelta > 0 || Input.IsGamePadButtonDownAny(GamePadButton.PadLeft))
                    {
                        if (Time.time > lastScrollWheeledTime + mwheelcooldown)
                            OnScroll(Equipped + 1);
                    }
                    if (G.S.Input.MouseWheelDelta < 0 || Input.IsGamePadButtonDownAny(GamePadButton.PadRight))
                    {
                        if (Time.time > lastScrollWheeledTime + mwheelcooldown)
                            OnScroll(Equipped - 1);
                    }

                    if (EquippedUsable is ActorUsable cur && cur.IsReady)
                    {
                        var joyshoot = Input.GetRightTriggerAny(0.2f) > 0.2f;
                        if (Input.IsMouseButtonPressed(MouseButton.Left) || (!shootDown && joyshoot) )  // This will allow for continuous shooting
                        {
                            cur.OnFireDown();
                        }
                        if (Input.IsMouseButtonDown(MouseButton.Left) || joyshoot)
                        {
                            cur.OnFireFrame();
                        }

                        if (Input.IsMouseButtonReleased(MouseButton.Left) || (shootDown && !joyshoot))
                        {
                            cur.OnFireUp();
                        }
                        shootDown = joyshoot;
                        if (Input.IsMouseButtonPressed(MouseButton.Right))
                        {
                            cur.OnAltFireDown();
                        }
                        if (Input.IsMouseButtonDown(MouseButton.Right))
                        {
                            cur.OnAltFireFrame();
                        }
                        if (Input.IsMouseButtonReleased(MouseButton.Right))
                        {
                            cur.OnAltFireUp();
                        }
                        if (Keybinds.GetKeyUp(Keybind.Drop))
                        {
                            cur.Drop();
                        }
                        if (Keybinds.GetKeyDown(Keybind.Reload))
                        {
                            cur.Reload();
                        }
                    }
                }
                await Script.NextFrame();
            }
        }

        void OnScroll(int newslot)
        {
            lastScrollWheeledTime = Time.time;
            if (newslot < 0) newslot = Inventory.Slots.Count - 1;
            else if (newslot >= Inventory.Slots.Count) newslot = 0;

            EquipWeapon(newslot);
        }
        public void OnValueChanged()
        {
            var slot = Cvars.Get<int>("equipped");
            TryEquip(slot);
        }

        protected override List<CvarMultiListenerInfo> GetCvars()
        {
            return new List<CvarMultiListenerInfo>
            {
                new CvarMultiListenerInfo
                {
                    Cvar = "equipped",
                    OnValueChanged = OnValueChanged,

                },
                new CvarMultiListenerInfo
                {
                    Cvar = "player",
                    OnValueChanged = Refresh,
                }
            };
        }
    }
}