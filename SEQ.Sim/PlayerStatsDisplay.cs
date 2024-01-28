using Stride.Core;
using Stride.Core.Serialization.Contents;
using Stride.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<PlayerStatsDisplay>))]
    public class PlayerStatsDisplay : CvarMultiListenerNoMB
    {
        string BoundId;
        UIElement El;
        public void Bind(string id, UIElement el)
        {
            BoundId = id;
            El = el;
            HpText.Init(el);
            hpcvar = id + ":hp";
            DoStart();
        }

        string hpcvar;

        public TextDisplay HpText = new TextDisplay { Ref = "hptext" };
        protected override List<CvarMultiListenerInfo> GetCvars()
        {
            return new List<CvarMultiListenerInfo>
            {
                new CvarMultiListenerInfo
                {
                    Cvar = BoundId,
                    OnValueChanged = OnChange,
                }
            };
        }

        void OnChange()
        {
            HpText.text = $"{Cvars.Get(hpcvar)}%";
        }

    }
}
