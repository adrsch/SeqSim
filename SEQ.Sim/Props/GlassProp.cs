using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
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
    public class GlassProp : StartupScript, IDamageable
    {

        public bool IsDead => true;
        public Actor Actor;

        public bool Penetrable => true;

        public bool CanWalkThrough => Actor.State.GetVar<int>("any") == 1;

        public bool UsesKnockback => false;

        public Vector3 HitboxCenter => Transform.Position;
        public ModelComponent Undamaged;
        public ModelComponent Model00;
        public ModelComponent Model01;
        public ModelComponent Model02;
        public ModelComponent Model10;
        public ModelComponent Model11;
        public ModelComponent Model12;

        public RigidbodyComponent RbFor(ModelComponent model) => model.Entity.Get<RigidbodyComponent>();

        public void Damage(DamageInfo info)
        {
            UpdateDestruction();

            var localPos = Transform.WorldToLocal(info.Point);
            if (localPos.x <  0 )
            {
                if (localPos.z < -0.4f)
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["00"] = "1";
                }
                else if (localPos.z < 0.4f)
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["01"] = "1";
                }
                else
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["02"] = "1";
                }
            }
            else
            {
                if (localPos.z < -0.4f)
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["10"] = "1";
                }
                else if (localPos.z < 0.4f)
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["11"] = "1";
                }
                else
                {
                    Actor.State.Vars["any"] = "1";
                    Actor.State.Vars["12"] = "1";
                }
            }
            Actor.State.OnChanged();
        }

        public override void Start()
        {
            base.Start();
            Actor.OnValueChanged += UpdateDestruction;
            UpdateDestruction();
        }

        public void DestroySegment(Entity ent)
        {
            if (ent == Model00.Entity)
            {
                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["00"] = "1";
            }
            else if (ent == Model01.Entity)
            {

                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["01"] = "1";
            }
            else if (ent == Model02.Entity)
            {

                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["02"] = "1";
            }
            else if (ent == Model10.Entity)
            {
                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["10"] = "1";
            }
            else if (ent == Model11.Entity)
            {

                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["11"] = "1";
            }
            else if (ent == Model12.Entity)
            {

                Actor.State.Vars["any"] = "1";
                Actor.State.Vars["12"] = "1";
            }
            Actor.State.OnChanged();
        }

        void UpdateDestruction()
        {
            if (Actor.State.GetVar<int>("any") == 1)
            {
                Undamaged.Enabled = false;
                RbFor(Undamaged).Enabled = false;
                Model00.Enabled = true;
                RbFor(Model00).Enabled = true;
                Model01.Enabled = true;
                RbFor(Model01).Enabled = true;
                Model02.Enabled = true;
                RbFor(Model02).Enabled = true;
                Model10.Enabled = true;
                RbFor(Model10).Enabled = true;
                Model11.Enabled = true;
                RbFor(Model11).Enabled = true;
                Model12.Enabled = true;
                RbFor(Model12).Enabled = true;
            }
            else
            {
                Undamaged.Enabled = true;
                RbFor(Undamaged).Enabled = true;
                Model00.Enabled = false;
                RbFor(Model00).Enabled = false;
                Model01.Enabled = false;
                RbFor(Model01).Enabled = false;
                Model02.Enabled = false;
                RbFor(Model02).Enabled = false;
                Model10.Enabled = false;
                RbFor(Model10).Enabled = false;
                Model11.Enabled = false;
                RbFor(Model11).Enabled = false;
                Model12.Enabled = false;
                RbFor(Model12).Enabled = false;
            }
            if (Actor.State.GetVar<int>("00") == 1)
            {
                Model00.Enabled = false;
                RbFor(Model00).Enabled = false;
            }
            if (Actor.State.GetVar<int>("01") == 1)
            {
                Model01.Enabled = false;
                RbFor(Model01).Enabled = false;
            }
            if (Actor.State.GetVar<int>("02") == 1)
            {
                Model02.Enabled = false;
                RbFor(Model02).Enabled = false;
            }
            if (Actor.State.GetVar<int>("10") == 1)
            {
                Model10.Enabled = false;
                RbFor(Model10).Enabled = false;
            }
            if (Actor.State.GetVar<int>("11") == 1)
            {
                Model11.Enabled = false;
                RbFor(Model11).Enabled = false;
            }
            if (Actor.State.GetVar<int>("12") == 1)
            {
                Model12.Enabled = false;
                RbFor(Model12).Enabled = false;
            }
        }
    }
}
