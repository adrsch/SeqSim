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
    public struct EquippedWeaponIncident
    {
        public IPerceptible Equipper;
    }
    public struct ShootingIncident
    {
        public IPerceptible Shooter;
        public IPerceptible GotShot;
    }
    public struct FlaggingIncident
    {
        public IPerceptible Flagger;
        public IPerceptible Flagged;
    }

    public class IncidentResponder
    {
        public void AddAllListeners()
        {
            EventManager.AddListener<ShootingIncident>(x =>
            {
                var current = FactionManager.GetRelation(x.Shooter.Faction, x.GotShot.Faction);
                var newrelation = Math.Clamp(current - 5, 0, 100);
                Logger.Log(Channel.Gameplay, LogPriority.Trace, $"Shooting incident: {x.Shooter.Faction} shot {x.GotShot.Faction}. Old relation: {current}. New relation: {newrelation}. Classes: {x.Shooter} {x.GotShot}");
                FactionManager.SetRelation(x.Shooter.Faction, x.GotShot.Faction, newrelation);

                IncidentNotifcations.S.Raise(Loc.Get("incshooting"), $"relation -5");
            });
            EventManager.AddListener<FlaggingIncident>(x =>
            {
                if (x.Flagged.Faction.GetRelation(PlayerAnimator.S.Faction) == RelationType.Violence)
                    return;

                var current = FactionManager.GetRelation(x.Flagger.Faction, x.Flagged.Faction);
                var newrelation = Math.Clamp(current - 1, 0, 100);
                Logger.Log(Channel.Gameplay, LogPriority.Trace, $"Flagging incident: {x.Flagger.Faction} flagged {x.Flagged.Faction}. Old relation: {current}. New relation: {newrelation}. Classes: {x.Flagger} {x.Flagged}");
                FactionManager.SetRelation(x.Flagger.Faction, x.Flagged.Faction, newrelation);

                IncidentNotifcations.S.Raise(Loc.Get("incflagging"), $"relation -1");
            });
        }
    }
}
