using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class TriggerTester : StartupScript
    {

        public override void Start()
        {
            base.Start();

            var trigger = Entity.Get<PhysicsComponent>();
            trigger.Collisions.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    //new collision
                    var collision = (Collision)args.Item;
                    //do something
                    Logger.Log(Channel.Gameplay, LogPriority.Info, $"ADDED COLLIDER: {collision.ColliderA.Entity.Name} | {collision.ColliderB.Entity.Name} ");
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    //old collision
                    var collision = (Collision)args.Item;
                    Logger.Log(Channel.Gameplay, LogPriority.Info, $"REMOVED COLLIDER: {collision.ColliderA.Entity.Name} | {collision.ColliderB.Entity.Name} ");

                    //do something
                }
            };
        }
    }
}
