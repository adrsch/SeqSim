using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

// TODO finish this
namespace SEQ.Sim
{
    public enum UISoundClip
    {
        None = 0,
        TextBeep = 1,
    }
    public class UIAudio : StartupScript
    {
        public static UIAudio S = new UIAudio();
        public AudioEmitterComponent AudioEmitterComponent;
        public Dictionary<UISoundClip, string> Clips = new Dictionary<UISoundClip, string>();
        public override void Start()
        {
            base.Start();

            UIAudio.S = this;
        }

        public static void Play(UISoundClip clip)
        {
            if (clip != UISoundClip.None)
            {
                S.AudioEmitterComponent.Oneshot(S.Clips[clip]);
            }
        }
    }
}
