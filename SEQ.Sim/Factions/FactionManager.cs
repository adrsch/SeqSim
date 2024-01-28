using SEQ;
using Stride.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

// TODO do not hardcode
namespace SEQ.Sim
{
    /*
    [System.Flags]
    public enum Faction
    {
        none = 0,
        comrade = 1 << 0,
        hostage = 1 << 1,
        police = 1 << 2,
        bystander = 1 << 3,
        creature = 1 << 4,
        military = 1 << 5,
        security = 1 << 6,
        press = 1 << 7,
        special = 1 << 8,

        Any = int.MaxValue,
    };

    */
    public class FactionRelations
    {
        //public Guid Id = Guid.NewGuid();
        public bool IsDirty;
        //public Faction ThisFaction;
        public Type Type;
        public string Cvar;
        // Just a provider
        public IFactionProvder Provider;

        Dictionary<Type, int> Relations = new Dictionary<Type, int>();

        public void Set(Type f, int r)
        {
            Relations[f] = r;
            IsDirty = false;
        }

        public int Get(Type t)
        {
            if (t == Type)
                return 100;
            if (IsDirty)
            {
                foreach (var other in FactionManager.S.Relations)
                {
                    Relations[other.Type] = Cvars.Get<int>($"{Cvar}:{other.Cvar}");
                }
                IsDirty = false;

            }
            if (Relations.ContainsKey(t))
                return Relations[t];
            return 50;
        }

        public RelationType Relation(Type t)
        {
            if (t == Type)
                return RelationType.Self;
            var r = Get(t);
            if (r < 15)
                return RelationType.Violence;
            if (r < 30)
                return RelationType.Unsure;
            if (r < 40)
                return RelationType.Unfriendly;
            if (r < 70)
                return RelationType.Neutral;
            return RelationType.Friendly;
        }

        public RelationType Relation(IFactionProvder t)
        {
            return Relation(t.GetType());
        }
    }

    public enum RelationType
    {
        Violence,
        Unsure,
        Unfriendly,
        Neutral,
        Friendly,
        Self,
    }

    public class FactionManager : ICvarListener
    {
        public static FactionManager S;
        public IncidentResponder IncidentResponder = new IncidentResponder();
        public FactionManager()
        {
            S = this;
            LoadFactions();
            IncidentResponder.AddAllListeners();
        }

        public List<FactionRelations> Relations = new();

        void LoadFactions()
        {
            var providers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IFactionProvder).IsAssignableFrom(p) && p.IsClass);

            var biggestInt = 1;
            foreach (var t in providers)
            {
                var inst = (IFactionProvder)Activator.CreateInstance(t);
                var rel = GetRelations(t);
                rel.Cvar = inst.Cvar();
                rel.Provider = inst;

                if (!Cvars.Listeners.Entries.ContainsKey(inst.Cvar()))
                {
                    Cvars.Listeners.Entries[inst.Cvar()] = new List<CvarListenerInfo>();
                }

                Cvars.Listeners.Entries[inst.Cvar()].Add(new CvarListenerInfo
                {
                    Listener = this,
                    OnValueChanged = () => rel.IsDirty = true,
                });

                rel.IsDirty = true;
            }
        }

        public static FactionRelations GetRelations(string f)
        {
            foreach (var r in S.Relations)
            {
                if (r.Cvar == f)
                    return r;
            }
            return default;
            //var newRel = new FactionRelations { Cvar = f };
            //S.Relations.Add(newRel);
            //return newRel;
        }

        public static FactionRelations GetRelations(Type t)
        {
            foreach (var r in S.Relations)
            {
                if (r.Type == t)
                    return r;
            }
            var newRel = new FactionRelations { Type = t };
            S.Relations.Add(newRel);
            return newRel;
        }

        public static FactionRelations GetRelations(IFactionProvder t)
        {
            return GetRelations(t.GetType());
        }

        public static void SetRelation(string fa, string fb, int v)
        {
            Cvars.Set($"{fa}:{fb}", v.ToString());
            Cvars.Set($"{fb}:{fa}", v.ToString());
        }
        public static void SetRelation(IFactionProvder fa, IFactionProvder fb, int v)
        {
            Cvars.Set($"{fa.Cvar}:{fb.Cvar}", v.ToString());
            Cvars.Set($"{fb.Cvar}:{fa.Cvar}", v.ToString());
        }


        public static int GetRelation(string fa, string fb)
        {
            return Cvars.Get<int>($"{fa}:{fb}");
        }


        public static int GetRelation(Type fa, Type fb)
        {
            return GetRelations(fa).Get(fb);
        }

        public static int GetRelation(IFactionProvder fa, Type fb)
        {
            return GetRelations(fa).Get(fb);
        }

        public static int GetRelation(IFactionProvder fa, IFactionProvder fb)
        {
            return GetRelation(fa, fb.GetType());
        }
    }
}