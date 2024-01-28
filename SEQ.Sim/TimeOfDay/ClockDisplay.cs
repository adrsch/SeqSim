using Stride.Core;
using Stride.Core.Serialization.Contents;
using Stride.UI;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<ClockDisplay>))]
    public class ClockDisplay
    {
        public TextDisplay ClockText = new TextDisplay
        {
            Ref = "clock"
        };
        // Start is called before the first frame updateClockDisplayClockDisplay

        public void Init(UIElement el)
        {
            ClockText.Init(el);

            ClockText.text = Clock.S.GetClockTime();
            Clock.S.OnMinute += () =>
            {
                ClockText.text = Clock.S.GetClockTime();
            };
        }

    }
}