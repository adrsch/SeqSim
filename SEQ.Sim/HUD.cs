using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class HUD : TemplateManager
    {
        public static HUD Sim { get; private set; }

        [DataMemberIgnore]
        public UIComponent UI;

        [DataMemberIgnore]
        public CrosshairManager CrosshairManager = new CrosshairManager();

        public bool IsSmall;

        public event Action<bool> OnSetScaling;

         public TextDisplay FocusText = new TextDisplay
        {
            Ref = "focustext",
            UseConsole = true,
            UseShadow = true,
            ShadowColor = Color.Black,
        };

        public ClockDisplay Clock = new ClockDisplay();
        public PlayerStatsDisplay PlayerStats =  new PlayerStatsDisplay();
        IncidentNotifcations Incidents = new IncidentNotifcations();

        public override void Start()
        {
            base.Start();
            Sim = this;
            UI = Entity.Get<UIComponent>();
            CrosshairManager.UI = UI;
            CrosshairManager.Center = UI.Page.RootElement.FindVisualChildOfType<ImageElement>("crosshaircenter");
            CrosshairManager.Left = UI.Page.RootElement.FindVisualChildOfType<ImageElement>("crosshairleft");
            CrosshairManager.Right = UI.Page.RootElement.FindVisualChildOfType<ImageElement>("crosshairright");
            CrosshairManager.Up = UI.Page.RootElement.FindVisualChildOfType<ImageElement>("crosshairup");
            CrosshairManager.Down = UI.Page.RootElement.FindVisualChildOfType<ImageElement>("crosshairdown");
            CrosshairManager.OnStart();

            var deckTap = UI.Page.RootElement.FindVisualChildOfType<TextBlock>("deckTapSelect");
            if (deckTap != null)
            {
                if (G.S.GetModule<ITouchModule>() is ITouchModule touchModule && touchModule.TouchEnabled)
                {
                    deckTap.Visibility = Visibility.Visible;
                    deckTap.Text = Loc.Get("decktapselect");
                }
                else
                {
                    deckTap.Visibility = Visibility.Collapsed;
                }
            }

            FocusText.Init(UI.Page.RootElement);
            Clock.Init(UI.Page.RootElement);
            Incidents.Bind(UI);

            EventManager.AddListener<InteractEvent>(evt =>
            {
                if (evt.Target is IFocusText ft)
                {
                    if (evt.Type == InteractType.Focus)
                        SetFocus(ft);
                    else if (evt.Type == InteractType.Unfocus)
                    {
                        ClearFocus();
                    }
                }
            });

            PlayerStats.Bind("player", UI.Page.RootElement);

            foreach (var b in Templates)
            {
                b.Init(UI);
            }

        }
        void SetFocus(IFocusText focus)
        {
            FocusText.Text = focus.GetText();
        }

        void ClearFocus()
        {
            FocusText.text = "";
        }

        public override void Update()
        {
            base.Update();
            FocusText.UseConsole = !G.S.DebugMode;
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
            if (MovementState.Inst.IsActive())
            {
                Show();
                CrosshairManager.DoUpdate(Time.deltaTime);
                FocusText.Update(Time.unscaledDeltaTime);
            }
            else if (GameStateManager.Inst.Active is Template)
            {
                Show();
                CrosshairManager.DoUpdate(Time.deltaTime);
                FocusText.Clear();
            }
            else
            {
                Hide();
            }
            DebugText.Print($"{Game.UpdateTime.FramePerSecond} fps", new Int2(100, 200));
        }

        void Hide()
        {
            UI.Page.Hide();
            UI.Page.RootElement.Visibility = Visibility.Collapsed;
        }

        void Show()
        {
            UI.Page.Show();
            UI.Page.RootElement.Visibility = Visibility.Visible;
        }
    }
}
