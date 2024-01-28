using Stride.Animations;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public enum ActorUsableType
    {
        Default = 0,
        Weapon = 1,
    }
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<ActorSpecies>))]
    [CategoryOrder(10, "Refs", Expand = ExpandRule.Always)]
    [CategoryOrder(20, "Item", Expand = ExpandRule.Auto)]
    [CategoryOrder(30, "Weapon", Expand = ExpandRule.Auto)]
    public class ActorSpecies : IActorSpecies
    {
        [Display(category: "Refs", order: 10)]
        public string Species { get; set; }
        [Display(category: "Refs", order: 20)]
        public Prefab Prefab { get; set; }

        [Display(category: "Item", order: 29)]
        public ActorUsableType UsableType;
        [Display(category: "Item", order: 30)]
        public Prefab FpPrefab;
        //[Display(category: "Item", order: 30)]
        //public Prefab AIPrefab;
        [Display(category: "Item", order: 40)]
        public Texture Icon;
        [Display(category: "Item", order: 50)]
        public bool Stackable;

        [Display(category: "Weapon", order: 10)]
        public string AmmoType;
        [Display(category: "Weapon", order: 20)]
        public int ClipSize;
        [Display(category: "Weapon", order: 30)]
        public bool IsRaycast;
        [Display(category: "Weapon", order: 31)]
        public ComputeCurveSamplerVector2 SpreadCurve = new ComputeCurveSamplerVector2();
        [Display(category: "Weapon", order: 33)]
        public int Damage;
        [Display(category: "Weapon", order: 34)]
        public float Knockback;
        [Display(category: "Weapon", order: 36)]
        public float FireDecibles;
        [Display(category: "Weapon", order: 37)]
        public bool IsFists;
        [Display(category: "Weapon", order: 39)]
        public float MaxRange = 999f;
        [Display(category: "Weapon", order: 40)]
        public float AIRangeMax = 50f;
        [Display(category: "Weapon", order: 41)]
        public float AIRangeMin = 0f;
    }

    public class SimSpeciesRegistry : ActorSpeciesRegistry
    {
        public static SimSpeciesRegistry Sim;
        [DataMember]
        public List<ActorSpecies> SpeciesList = new List<ActorSpecies>();

        public override void Start()
        {
            Sim = this;
            base.Start();
            foreach (var s in SpeciesList)
            {
                Species[s.Species] = s;
            }
        }

        public override List<IActorSpecies> GetSpecies()
        {
            return SpeciesList.Cast<IActorSpecies>().ToList();
        }

        public static bool TryGetSpecies(string id, out ActorSpecies species)
        {
            IActorSpecies sp;
            var success = S.Species.TryGetValue(id, out sp);
            // TODO not good
            species = sp as ActorSpecies;
            return success;
        }
    }
}