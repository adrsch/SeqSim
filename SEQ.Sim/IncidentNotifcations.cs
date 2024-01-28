using Stride.Engine;
using Stride.UI.Panels;
using Stride.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine.Design;
using Stride.UI.Controls;
using BulletSharp.SoftBody;
using System.Drawing;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class IncidentNotifcations
    {
        public static IncidentNotifcations S;
         string PrefabId = "incidentprefab";
         string TitleId = "inctitle";
         string EffectId = "incfx";
        UIElement Prefab;
        TextBlock TitlePrefab;
        TextBlock EffectPrefab;

        UIComponent UI;
        StackPanel Stack;
        public void Bind(UIComponent ui)
        {
            UI = ui;
            S = this;

            Prefab = UI.Page.RootElement.FindVisualChildOfType<Grid>(PrefabId);
            TitlePrefab = Prefab.FindVisualChildOfType<TextBlock>(TitleId);
            EffectPrefab = Prefab.FindVisualChildOfType<TextBlock>(EffectId);
            Prefab.Visibility = Visibility.Collapsed;
            Stack = Prefab.Parent as StackPanel;
        }

        public void Raise(string title, string desc)
        {
            var notif = new Grid();
            notif.BackgroundColor = Stride.Core.Mathematics.Color.Red;
            notif.Width = 512f;
            Stack.Children.Add(notif);
            notif.Visibility = Visibility.Visible;
            var titleEl = new TextBlock();
            titleEl.Text = title;
            titleEl.Font = TitlePrefab.Font;
            titleEl.Margin = TitlePrefab.Margin;
            titleEl.VerticalAlignment = VerticalAlignment.Center;
            var effectEl = new TextBlock();
            effectEl.Text = desc;
            effectEl.Font = EffectPrefab.Font;
            effectEl.Margin = EffectPrefab.Margin;
            effectEl.TextAlignment = Stride.Graphics.TextAlignment.Right;
            effectEl.VerticalAlignment = VerticalAlignment.Center;
            notif.Children.Add(titleEl);
            notif.Children.Add(effectEl);
            Stack.Children.Add(notif);
            G.S.Script.AddTask(async () =>
            {
                await Task.Delay(5000);
                Stack.Children.Remove(notif);
            });
        }

    }
}
