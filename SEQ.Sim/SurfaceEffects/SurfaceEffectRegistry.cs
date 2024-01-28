using Stride.Core.Annotations;
using Stride.Core.Serialization.Contents;
using Stride.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Physics;
using Stride.Core.Mathematics;
using BulletSharp;
using Stride.Animations;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<SurfaceEffect>))]
    //[CategoryOrder(10, "Refs", Expand = ExpandRule.Always)]
    //[CategoryOrder(20, "Item", Expand = ExpandRule.Auto)]
    //[CategoryOrder(30, "Weapon", Expand = ExpandRule.Auto)]
    public class SurfaceEffect
    {
        public SurfaceType Surface;
        public Prefab Prefab;
        public int Lifetime = 10000;
    //    public string Oneshot;
    }

    public class SurfaceEffectRegistry : StartupScript
    {
        public static SurfaceEffectRegistry S;
        public List<SurfaceEffect> Footsteps = new List<SurfaceEffect>();

        public List<SurfaceEffect> BulletImpacts = new List<SurfaceEffect>();
        public Prefab BulletHole;
        public Prefab Tracer;
        public override void Start()
        {
            base.Start();
            S = this;
        }

        public void BulletImpact(HitResult res, bool ispen, Vector3 forward)
        {
            if (!res.Succeeded || res.Collider == null) return;
            foreach (var fx in BulletImpacts)
            {
                if (fx.Surface == res.Collider.SurfaceType)
                {
                    var ent = fx.Prefab.InstantiateTemporary(Entity.Scene, fx.Lifetime);
                    ent.Transform.WorldPosition = res.Point;
                    //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
                    ent.Transform.Rotation = Quaternion.LookRotation(in Vector3.forward, in res.Normal);

                    if (ent.Get<AudioEmitterComponent>() is AudioEmitterComponent emitter)
                    {
                        emitter.Oneshot("hit");
                    }
                }
            }

            if ((res.Collider is RigidbodyComponent) || res.Collider.SurfaceType == SurfaceType.Player
                 || res.Collider.SurfaceType == SurfaceType.Flesh)
            {
                return;
            }
            if (!ispen)
            {
                SpawnPermaHole(res, forward, res.Collider.SurfaceType);
                /*
                TemporaryBulletHole(res);

                // TODO perma holes
                switch (res.Collider.SurfaceType)
                {
                    case SurfaceType.Glass:
                        return;
                    case SurfaceType.Snow:
                        break;
                    case SurfaceType.Concrete:
                        break;
                }*/
            }
        }

        public void ForceEffect(HitResult res)
        {
            foreach (var fx in BulletImpacts)
            {
                if (fx.Surface == res.Collider.SurfaceType)
                {
                    var ent = fx.Prefab.InstantiateTemporary(Entity.Scene, fx.Lifetime);
                    ent.Transform.WorldPosition = res.Point;
                    //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
                    ent.Transform.Rotation = Quaternion.LookRotation(in Vector3.forward, in res.Normal);

                    if (ent.Get<AudioEmitterComponent>() is AudioEmitterComponent emitter)
                    {
                        emitter.Oneshot("hit");
                    }
                }
            }
        }


        void SpawnPermaHole(HitResult res, Vector3 forward, SurfaceType t)
        {
            var st  = ActorSpeciesRegistry.SpawnNewState("bullethole");
            if (st == null)
            {
                Logger.Log(Channel.Data, LogPriority.Warning, $"No bullet hole prefab found.");
                return;

            }
            st.Position = res.Point + res.Normal * 0.05f;
            var ToSaveRotation = Quaternion.LookRotation(res.Normal, forward);//(in Vector3.forward, in res.Normal);
            st.Rotation = Quaternion.Identity;
            switch (t)
            {
                default:
                    st.Vars["fadehour"] = "9999";
                    st.Vars["killhour"] = "9999";
                    break;
                case SurfaceType.Snow:
                    st.Vars["fadehour"] = (Clock.S.HoursAndDays + 2).ToString();
                    st.Vars["killhour"] = (Clock.S.HoursAndDays + 6).ToString();
                    break;
                case SurfaceType.Stone:
                    st.Vars["fadehour"] = (Clock.S.HoursAndDays + 2).ToString();
                    st.Vars["killhour"] = (Clock.S.HoursAndDays + 6).ToString();
                    break;
            }
            st.SetProjectionMeshOrientation(ToSaveRotation);
            ActorSpeciesRegistry.FromState(st);
            //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
          //  ent.Transform.Rotation = Quaternion.LookRotation(in Vector3.forward, in res.Normal);
        }

        void TemporaryBulletHole(HitResult res)
        {
            var ent = BulletHole.InstantiateTemporary(Entity.Scene, 20000);
            ent.Transform.WorldPosition = res.Point + res.Normal * 0.05f;
            //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
            ent.Transform.Rotation = Quaternion.LookRotation(in Vector3.forward, in res.Normal);
        }
        public void DoTracer(Vector3 start, Vector3 end, Vector3 forward, Quaternion rotation)
        {

            var ent = Tracer.InstantiateTemporary(Entity.Scene, 5000);
            G.S.Script.AddTask(async () => await DoTracerAsync(ent, start, end, forward.Normalized, rotation));

        }


        public async Task DoTracerAsync(Entity ent, Vector3 start, Vector3 end, Vector3 forward, Quaternion rotation)
        {
            ent.Transform.WorldPosition = start;
            //ent.Transform.Rotation = Quaternion.LookAt(ref ent.Transform.Rotation, res.Normal);
            ent.Transform.Rotation = rotation;
            var distance = Vector3.Distance(start, end);
            float elapsed = 0f;
            var total = 10f;
            while (elapsed < total)
            {
                var progress = elapsed / total;
                var position = start + forward * (distance * progress);
                ent.Transform.WorldPosition = position;
                ent.Transform.Scale = new Vector3(ent.Transform.Scale.x, ent.Transform.Scale.y, ((1f - progress) * distance) * MathF.Sin(MathF.PI * progress));
                await Script.NextFrame();
                elapsed += Time.deltaTime;
            }
        }
    }
}
