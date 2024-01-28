using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public static class SimCommands
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "relation", new CommandInfo
                {
                    Params = [typeof(string),typeof(string)],
                    Exec = async args =>
                    {
                        if ((int?)args[2] != null)
                            FactionManager.SetRelation((string)args[0], (string)args[1], ((int?)args[2]).Value);
                        else
                        {
                            Logger.Print($"{FactionManager.GetRelation((string)args[0], (string)args[1])}");
                        }
                    },

                    Help = "Set cvars for both to be same relation for faction",
                    OptionalParams = [typeof(int?)]
                }
            },
            {
                "noclip", new CommandInfo
                {
                    Exec = async args =>
                    {
                        PhysicsConstants.Noclip = !PhysicsConstants.Noclip;
                    },
                }
            },
            {
                "god", new CommandInfo
                {
                    Exec = async args => DeathScreen.GODMODE = !DeathScreen.GODMODE,
                    Help = "GODMODE"
                }
            },
            {
                "killall", new CommandInfo
                {
                    Exec = async args => {
                        foreach (var a in ActorRegistry.Active.Values)
                        {
                            if (a.State.SeqId != "player" && a.Entity.Get<SimDamageable>() is SimDamageable dm)
                                dm.Damage(new DamageInfo{Amount = 1000000});
                        }
                    },
                    Help = "kills all the ai"
                }
            },
            #region physics
            {
                "gravity", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.Gravity = (float)args[0],
                }
            },
            {
                "groundspeed", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.Speed = (float)args[0],
                }
            },
            {
                "groundaccel", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.Accel = (float)args[0],
                }
            },
            {
                "groundstop", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.StopSpeed = (float)args[0],
                }
            },
            {
                "airspeed", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>PhysicsConstants.AirSpeed = (float)args[0],
                }
            },
            {
                "airaccel", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.AirAccel = (float)args[0],
                }
            },
            {
                "airstop", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.AirStopSpeed = (float)args[0],
                }
            },
            {
                "strafeaccel", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.StrafeAccel = (float)args[0],
                }
            },
            {
                "sprintspeed", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.SprintSpeed = (float)args[0],
                }
            },
            {
                "sprintaccel", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.SprintAccel = (float)args[0],
                }
            },
            {
                "sprintstop", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.SprintStopSpeed = (float)args[0],
                }
            },
            {
                "crouchspeed", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.CrouchSpeed = (float)args[0],
                }
            },
            {
                "crouchaccel", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.CrouchAccel = (float)args[0],
                }
            },
            {
                "crouchstop", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.CrouchStopSpeed = (float)args[0],
                }
            },
            {
                "friction", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.Friction =(float) args[0],
                }
            },
            {
                "aircontrol", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.AirControl =(float) args[0],
                }
            },
            {
                "jump", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.JumpForce =(float) args[0],
                }
            },
            {
                "autobhop", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args => PhysicsConstants.Autobhop =(bool) args[0],
                }
            },
            {
                "noclipspeed", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.NoclipSpeed = (float)args[0],
                }
            },
            {
                "noclipspeedsprint", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.NoclipSprint = (float)args[0],
                }
            },
            {
                "noclipspeedcrouch", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.NoclipCrouch = (float)args[0],
                }
            },
            {
                "antinoobhop", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => PhysicsConstants.AntiNoobhopTime = (float)args[0],
                }
            },
            #endregion
#region time
            {
                "applytime", new CommandInfo
                {
                    Exec = async args => Clock.S.ApplyTime(),
                }

            },
#endregion
            //dialogue
            {
                "portrait", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => Portraits.S.Set("portrait", (string)args[0]),
                    Help = "Set the portrait image template"
                }
            },
            // TODO move to CommandsTemplate
            {
                "image", new CommandInfo
                {
                    Params = [typeof(string), typeof(string)],
                    Exec = async args => Portraits.S.Set((string)args[0], (string)args[1]),
                    Help = "Set any image template"
                }
            },
        };
    }
}
