// MIT License

using Stride.Core;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Graphics;
using Stride.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<PortraitTextureInfo>))]
    public class PortraitTextureInfo
    {
        public string Name;
        public VideoComponent Video;
        public Texture Tex;
    }
    public class Portraits : StartupScript
    {
        public List<PortraitTextureInfo> Images = new();

        public static Portraits S;
        public override void Start()
        {
            S = this;
            base.Start();
            foreach (var info in Images)
            {
                // confirm this is set
                if (info.Video != null)
                    info.Tex = info.Video.Target;
            }

            EventManager.AddListener<EndTemplateEvent>(x => StopAll());
        }

        public void StopAll()
        {
            foreach (var info in Images)
            {
                if (info.Video != null &&
                    (info.Video.Instance.PlayState == Stride.Media.PlayState.Playing)
                    || // todo not sure how paused is effected
                    (info.Video.Instance.PlayState == Stride.Media.PlayState.Paused))
                {
                    info.Video.Instance.Stop();
                }
            }
        }

        public void Set(string name, string img)
        {
            foreach (var info in Images)
            {
                if (info.Name == img)
                {
                    info.Video?.Instance.Play();
                    Template.Current?.SetImage(name, info.Tex);
                    return;
                }
            }
            // clear image
            // ie pass none
            Template.Current?.SetImage(name, null);
        }
    }
}
