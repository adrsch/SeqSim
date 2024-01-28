using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class DeathScreen : CvarListenerBase, IGameStateController
    {
        public UIComponent UI;

        UIElement RetryButton;

        public bool IsSmall;

        public event Action<bool> OnSetScaling;

        public TextDisplay DeathText = new TextDisplay { Ref = "deathtext" };

        public InteractionState GetInteractionState()
        {
            return InteractionState.GUI;
        }

        public PointerState GetPointerState()
        {
            return PointerState.GUI;
        }

        public void OnGainControl()
        {
            UI.Enabled = true;
            UI.Page.Show();
            SetSize();
        }

        public void OnLoseControl()
        {
            UI.Enabled = false;
            UI.Page.Hide();
        }
        public static bool GODMODE;
        public override void OnValueChanged()
        {
            if (!GODMODE && Cvars.Get<int>("player:hp") <= 0)
            {
                GameStateManager.Push(this);
                DeathText.text = "DEATH";
            }
            else
            {
                if (GameStateManager.Inst.Active == this)
                {
                    DeathText.text = "";
                }
                GameStateManager.Remove(this);
            }
        }
        public override void OnStart()
        {
            RetryButton = UI.Page.RootElement.FindVisualChildOfType<UIElement>("retrybutton");
            RetryButton.TouchDown += (x, y) =>
            {
                if (this.IsActive())
                    DoRetry();
            };
            DeathText.Init(UI.Page.RootElement);
            DeathText.Text = "";
        }

        public void DoRetry()
        {
            Shell.Exec("newsave");
        }


        public override void Update()
        {
            if (GameStateManager.Inst.Active == this)
            {
                SetSize();
                DeathText.Update(Time.deltaTime);
            }
        }
        void SetSize() {
            Input.UnlockMousePosition();
            Game.IsMouseVisible = true;


            var size = G.S.IsFullscreen ? Game.Window.PreferredFullscreenSize : Game.Window.PreferredWindowedSize;
            var asVector3 = new Vector3(size.X, size.Y, 512);
            UI.Size = asVector3;
            UI.Resolution = asVector3;
            if (asVector3.x < 1550)
            {
                if (!IsSmall)
                {
                    IsSmall = true;
                    OnSetScaling?.Invoke(IsSmall);
                }
            }
            else if (IsSmall)
            {
                IsSmall = false;
                OnSetScaling?.Invoke(IsSmall);
            }
    }

        protected override string GetCvar()
        {
            return "player:hp";
        }
    }
}
