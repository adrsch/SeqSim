using Stride.Core.Annotations;
using Stride.Core;
using Stride.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Reflection;

namespace SEQ.Sim
{

    [InlineProperty]
    public interface IFactionProvder
    {
        string Cvar();
    }

    public static class FactionExtensions
    {
        public static bool CaresAboutKilling(this IFactionProvder a, IFactionProvder other) => a.CaresAboutKilling(other.GetType());
        public static bool CaresAboutKilling(this IFactionProvder a, Type other)
        {
            var rel = FactionManager.GetRelations(a.GetType()).Relation(other);
            return rel == RelationType.Unsure || rel == RelationType.Unfriendly;
        }

        public static bool CanTalk(this IFactionProvder a, IFactionProvder other) => a.CanTalk(other.GetType());
        public static bool CanTalk(this IFactionProvder a, Type other)
        {
            var rel = FactionManager.GetRelations(a.GetType()).Relation(other);
            return rel == RelationType.Self || rel == RelationType.Friendly || rel == RelationType.Neutral || rel == RelationType.Unsure;
        }

        public static RelationType GetRelation(this IFactionProvder a, IFactionProvder other)
        {
            return FactionManager.GetRelations(a.GetType()).Relation(other.GetType());
        }

        public static RelationType GetRelation(this IFactionProvder a, Type other)
        {
            return FactionManager.GetRelations(a.GetType()).Relation(other);
        }

        public static int Get(this IFactionProvder a, IFactionProvder other)
        {
            return FactionManager.GetRelations(a.GetType()).Get(other.GetType());
        }

        public static int Get(this IFactionProvder a, Type other)
        {
            return FactionManager.GetRelations(a.GetType()).Get(other);
        }
    }
}
