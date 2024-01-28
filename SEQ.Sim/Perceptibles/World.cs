using Stride.Core.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public interface IWorldObject
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 Velocity { get; set; }
    }
    public class World
    {
        static World _Current;
        public static World Current
        {
            get
            {
                if (_Current == null)
                    _Current = new World();
                return _Current;
            }
            set
            {
                _Current = value;
            }
        }

        // TODO: slow
        HashSet<IWorldObject> Objects = new HashSet<IWorldObject>();

//        public List<IVisionTarget> VisionTargets = new List<IVisionTarget>();

  //      public List<AudioEmitter> AudioEmitters = new List<AudioEmitter>();

        public void Register(IWorldObject ob)
        {
     /*       if (ob is IVisionTarget ivt)
            {
                VisionTargets.Add(ivt);
            }
            else if (ob is AudioEmitter ae)
            {
                AudioEmitters.Add(ae);
            }
            else
            {*/
             //   Objects.Add(ob);
           // }
        }

        public void Register<T>(T obj) where T : IWorldObject
        {
            //   if (!Objects.ContainsKey(typeof(T)))
            //     Objects[typeof(T)] = new List<IWorldObject>();

            // Objects[typeof(T)].Add(obj);'
            //  Objects.Add(obj);
           // Register(obj);
        }

        public List<IPerceptible> Perceptibles = new List<IPerceptible>();
        public static void RegisterPerceptible(IPerceptible p)
        {
            Current.Perceptibles.Add(p);
        }
        public static void RemovePerceptible(IPerceptible p)
        {
            Current.Perceptibles.Remove(p);
        }


        public void Remove(IWorldObject ob)
        {

     /*       if (ob is IVisionTarget ivt)
            {
                VisionTargets.Remove(ivt);
            }
            else if (ob is AudioEmitter ae)
            {
                AudioEmitters.Remove(ae);
            }
            else
            {*/
         //       Objects.Remove(ob);
            //}
        }

        public IEnumerable<T> All<T>()
        {
            //  if (Objects.ContainsKey(typeof(T)))
            //     return Objects[typeof(T)].Cast<T>();
            // return new List<T>();
            return Objects.Where(x => {
                return typeof(T).IsAssignableFrom(x.GetType());
            }).Select(x => (T)x);
        }

        public static void Reset()
        {
            Current = null;
        }
    }
}
