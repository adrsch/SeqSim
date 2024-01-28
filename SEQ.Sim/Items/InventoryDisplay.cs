using System.Collections;
using System.Collections.Generic;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI;
using Stride.Input;
using Stride.Engine.Design;
using Stride.UI.Panels;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class InventoryDisplay : AysncCvarListenerBase
    {
        [DataMemberIgnore]
        public UIComponent UI;


        public string BoundEntity;

        [DataMemberIgnore]
        public List<InventorySlot> Slots = new List<InventorySlot>();

        public string SlotPrefab = "slotprefab";

        public InventorySlot Prefab;
        public int SlotCount;

        public bool IsPlayerInventory;

        public GUISelectionFollower Follower = new GUISelectionFollower();


        public override async Task BeforeInit()
        {
            while (ActorState.Get(BoundEntity) == null)
            {
                await Script.NextFrame();
            }

            UI = Entity.Get<UIComponent>();
            Follower.DoInit(UI);

            if (SlotCount > 0)
            {
                Prefab = new InventorySlot(UI.Page.RootElement.FindVisualChildOfType<Grid>(SlotPrefab), BoundEntity, Follower, 0, this);
                Slots.Add( Prefab);
                var stack = Prefab.Element.Parent as StackPanel;
                for (var i = 1; i < SlotCount; i++)
                {
                    var el = UICloner.Clone(Prefab.Element);
                   stack.Children.Add( el );
                    Slots.Add(new InventorySlot(el, BoundEntity, Follower, i, this));
                }
            }

            if (IsPlayerInventory)
            {
                UpdatePlayerBindings();

                EventManager.AddListener<SystemPrefsUpdatedEvent>(evt => UpdatePlayerBindings());
            }
        }

        void UpdatePlayerBindings()
        {
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot1))
                Slots[0].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot1].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot2))
                Slots[1].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot2].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot3))
                Slots[2].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot3].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot4))
                Slots[3].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot4].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot5))
                Slots[4].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot5].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot6))
                Slots[5].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot6].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot7))
                Slots[6].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot7].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot8))
                Slots[7].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot8].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot9))
                Slots[8].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot9].GetNameForKey()}]";
            if (Keybinds.S.Bindings.ContainsKey(Keybind.Slot10))
                Slots[9].Hotkey.Text = $"[{Keybinds.S.Bindings[Keybind.Slot10].GetNameForKey()}]";
        }

        public string SelectCvar = "equipped";
        public void Select(int slot)
        {
            Cvars.Set(SelectCvar, slot.ToString());
        }

        public override void OnValueChanged()
        {
            var entity = ActorState.Get(BoundEntity);
            var i = 0;
            foreach (var slot in Slots)
            {
                ActorState itemState = null;
                if (i < entity.Children.Count)
                {
                    var c = entity.Children[i];
                    itemState = ActorState.Get(c);
                }
                slot.Bind(itemState);
                i++;
            }
        }

        protected override string GetCvar()
        {
            return BoundEntity;
        }

        public override async Task AfterInit()
        {
            HUD.Sim.OnSetScaling += SetScaling;
            SetScaling(HUD.Sim.IsSmall);
            while (true)
            {
                Follower.OnUpdate(Time.deltaTime);
                await Script.NextFrame();
            }
        }

        void SetScaling(bool small)
        {
            foreach (var s in Slots)
            {
                s.Outline.Width = s.Outline.MaximumWidth = s.Outline.MinimumWidth =
                s.Outline.Height = s.Outline.MaximumHeight = s.Outline.MinimumHeight = small ? 100 : 128;
            }
            Follower.ElementWidth = small ? 100 : 128;
        }
    }
}
