using System.Collections;
using System.Collections.Generic;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using SEQ.Script;
using SEQ.Script.Core;

using SEQ.Sim;using SEQ.Sim;

namespace SEQ.Sim
{
    public class InventorySlot : ICvarListener
    {
        public ImageElement Icon;
        public ActorState BoundState;
        public TextDisplay Name = new TextDisplay { Ref = "name" };
        public TextDisplay Desc = new TextDisplay { Ref = "desc" };
        public TextDisplay Quantity = new TextDisplay { Ref = "quantity" };
        public TextDisplay CurrentMagazine = new TextDisplay { Ref = "currentmagazine" };
        public TextDisplay AmmoType = new TextDisplay { Ref = "ammotype" };
        public TextDisplay AmmoTotal = new TextDisplay { Ref = "ammototal" };
        //  public Color SelectColor = new Color(200, 15, 15, 255);
        public Color SelectColor = new Color(235, 15, 15, 255);
        public Color DefaultColor = Color.White;
        public ImageElement Outline;
        public UIElement ShowIfGun;

        public TextDisplay Hotkey = new TextDisplay { Ref = "hotkey" };

        public UIElement Element;


        ActorState ActorState;
        GUISelectionFollower Follower;
        int Index;
        InventoryDisplay Inv;
        public InventorySlot(UIElement el, string actor, GUISelectionFollower fol = null, int index = 0, InventoryDisplay dis = null)
        {
            Inv = dis;
            Follower = fol;
            Element = el;
            Hotkey.Init(el);
            Outline = el.FindVisualChildOfType<ImageElement>("slotborder");
            AmmoTotal.Init(el);
            AmmoType.Init(el);
            CurrentMagazine.Init(el);
            Quantity.Init(el);
            Desc.Init(el);
            Name.Init(el);
            ShowIfGun = el.FindVisualChildOfType<UIElement>("ifgun");
            Icon = el.FindVisualChildOfType<ImageElement>("icon");
            ActorState = ActorState.Get(actor);
            Index = index;
            if (AmmoTotal != null)
            {
                ActorState.AddListener(new CvarListenerInfo
                {
                    Listener = this,
                    OnValueChanged = () => UpdateAmmoTotal()
                });
            }
            if (Icon.Source is SpriteFromTexture t)
                t.Texture = null;

            ClearSelected();
            Index = index;

            if (Inv != null)
            {
                Outline.TouchDown += (x, y) =>
                {
                    if (!PlayerController.S.Input.IsMousePositionLocked
                    && (G.S.GetModule<ITouchModule>() is ITouchModule touchModule && touchModule.TouchEnabled))
                        Inv.Select(Index);
                };
            }
        }

        void UpdateAmmoTotal()
        {
            if (BoundState != null && BoundState.GetSpecies() is ActorSpecies sp && !string.IsNullOrWhiteSpace(sp.AmmoType)
                && ActorState.Vars.ContainsKey(sp.AmmoType))
            {
                AmmoTotal.Text = ActorState.Vars[sp.AmmoType];
            }
            else
            {
                AmmoTotal.Text = "";
            }
        }

        ~InventorySlot()
        {
            if (AmmoTotal != null)
            {
                ActorState.RemoveListener(this);
            }
        }

        public void SetAsSelected()
        {
            if (Follower != null)
                Follower.SetTarget(Index);
            if (Outline != null)
            {
                Outline.Color = SelectColor;
                Hotkey.Color = SelectColor;

                if (Follower != null)
                    Follower.SetColor(SelectColor);
            }
        }

        public void ClearSelected()
        {
            if (Outline != null)
            {
                Outline.Color = Color.Black;
                Hotkey.Color = DefaultColor;
            }
        }
        public void Bind(ActorState state)
        {
            if (BoundState != null)
            {
                if (CurrentMagazine != null)
                {
                    BoundState.RemoveListener(this);
                }
            }
            if (state == null)
            {
                Clear();
            }
            else if (string.IsNullOrEmpty(state.Species))
            {
                Logger.Log(Channel.Data, LogPriority.Warning, $"empty state id for {state.SeqId} on slot");
            }
            else if (SimSpeciesRegistry.TryGetSpecies(state.Species, out var pool))
            {
                if (AmmoType != null)
                {
                    if (!string.IsNullOrWhiteSpace(pool.AmmoType))
                        AmmoType.Text = Loc.Get(pool.AmmoType);
                    else
                        AmmoType.Text = "";
                }
                if (CurrentMagazine != null)
                {
                    if (state.Vars.TryGetValue("ammo", out var mag))
                    {
                        CurrentMagazine.Text = mag;
                        state.AddListener(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = () => CurrentMagazine.Text = state.Vars["ammo"]
                        });
                        if (ShowIfGun != null)
                            ShowIfGun.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CurrentMagazine.Text = "";
                        if (ShowIfGun != null)
                            ShowIfGun.Visibility = Visibility.Collapsed;
                    }
                }
                if (AmmoTotal != null)
                    UpdateAmmoTotal();

                if (Icon != null)
                {
                    if (pool.Icon != null && Icon.Source is SpriteFromTexture t)
                    {
                        t.Texture = pool.Icon;
                        Icon.Color = new Color(1, 1, 1, 1);
                    }
                    else
                        Logger.Log(Channel.Data, LogPriority.Warning, $"No icon for {state.SeqId} {state.Species}");
                }
                if (Name != null)
                {
                    Name.Text = state.GetNameWithQuantity();
                }
                if (Desc != null)
                {
                    Desc.Text = state.GetDescription();
                }
                if (Quantity != null)
                {
                    if (state.Quantity > 1)
                        Quantity.Text = state.Quantity.ToString();
                    else
                        Quantity.Text = "";
                }
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Error, $"No pooler for {state.SeqId} {state.Species}");

            }
            BoundState = state;

            if (AmmoTotal != null)
                UpdateAmmoTotal();
        }

        public void Clear()
        {
            if (ShowIfGun != null)
                ShowIfGun.Visibility = Visibility.Collapsed;
            if (CurrentMagazine != null)
                CurrentMagazine.Text = "";
            if (AmmoTotal != null)
                AmmoTotal.Text = "";
            if (AmmoType != null)
                AmmoType.Text = "";
            if (Icon != null)
            {
                Icon.Color = new Color(0, 0, 0, 0);
            }
            if (Name != null)
            {
                Name.Text = "";
            }
            if (Desc != null)
            {
                Desc.Text = "";
            }
            if (Quantity != null)
            {
                Quantity.Text = "";
            }
            BoundState = null;
        }
    }
}