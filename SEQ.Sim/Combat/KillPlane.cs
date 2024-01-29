using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Sim
{
    public class KillPlane : SyncScript
    {
        public override void Update()
        {
            foreach (var p in World.Current.Perceptibles)
            {
                if (!p.Damageable.IsDead && p.GetPosition().y < Transform.WorldPosition.y)
                {
                    p.Damageable.Damage(new DamageInfo
                    {
                        Amount = 999999,
                    });
                }
            }
        }
    }
}
