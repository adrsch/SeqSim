using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

// TODO: This isn't finished or working

namespace SEQ
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<GUISelectionFollower>))]
    public class GUISelectionFollower
    {
        public string ElementName;
        public float SmoothTime = 0.2f;
        int Target;

        [DataMemberIgnore]
        public bool IsActive;

        ImageElement Element;

        UIComponent UI;

        public void SetColor(Color color) => Element.Color = color;

        public void DoInit(UIComponent ui)
        {
            UI = ui;
            if (!string.IsNullOrWhiteSpace(ElementName))
            {
                Element = ui.Page.RootElement.FindVisualChildOfType<ImageElement>(ElementName);
                IsActive = Element != null;
            }
        }
        public void SetTarget(int t)
        {
            Target = t;
        }
        float vel;

        const float centerPosition = 640;//634;
        float width => ElementWidth + BorderWidth;

        [DataMemberIgnore]
        public float ElementWidth = 128;

        [DataMemberIgnore]
        public float BorderWidth = 10;
        public void  OnUpdate(float dt)
        {
            if (!IsActive)
                return;

            var element0 = centerPosition - 4.5f * width;
            var position = element0 + Target * width;

            var lerped = MathUtil.CriticalDamp(Element.Margin.Left, position, ref vel, SmoothTime, dt);
            Element.Margin = new Thickness(lerped, Element.Margin.Top, 1280 - lerped, Element.Margin.Bottom);
        }
    }
}