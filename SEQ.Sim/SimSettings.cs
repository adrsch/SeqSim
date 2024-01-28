using SEQ.Script.Core;
using Stride.Core;
using Stride.Engine.Design;
using Stride.Physics;
using Stride.Core;
using Stride.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Sim
{
    // TODO: Use this
    [DataContract]
    [Display("Sim")]
    public class SimSettings : Configuration
    {
    }
    public static partial class Settings
    {
        public static SimSettings Sim
        {
            get
            {
                var settings = Sequencer.S.Services.GetSafeServiceAs<IGameSettingsService>()?.Settings;
                return settings?.Configurations?.Get<SimSettings>() ?? new SimSettings();
            }
        }
    }
}