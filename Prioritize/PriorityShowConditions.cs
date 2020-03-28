using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Prioritize
{
    [StaticConstructorOnStartup]
    public static class PriorityShowConditions
    {
        private static List<PriorityShowCondition> Conditions = new List<PriorityShowCondition>();

        private static List<FloatMenuOption> CachedOptions = new List<FloatMenuOption>();

        public static PriorityShowCondition DefaultCondition;

        static PriorityShowConditions() 
        {
            var defcond = new PriorityShowCondition(delegate (Thing t)
            {
                Map map = t.Map;
                if (map == null)
                {
                    Log.ErrorOnce("Thing's Map is null", "P_TMN".GetHashCode());
                    return false;
                }

                var res = t is Blueprint || t is Frame || map.designationManager.DesignationOn(t) != null || map.listerHaulables.ThingsPotentiallyNeedingHauling().Contains(t);

                if (res) return true;

                if (t.Position.IsValid && map.designationManager.HasMapDesignationAt(t.Position))
                {
                    return true;
                }

                Building bui = t as Building;
                if (bui != null && t.Position.IsValid)
                {
                    if (bui.def.building.repairable && t.def.useHitPoints && t.HitPoints != t.MaxHitPoints && t.Map?.areaManager.Home[t.Position] == true) res = true;
                    if (bui.def == ThingDefOf.DeepDrill) res = true;
                }

                return res;
            }, "P_Auto".Translate());
            DefaultCondition = defcond;

            Conditions.Add(defcond);

            Conditions.Add(new PriorityShowCondition(delegate (Thing t) 
            {
                return t is Blueprint || t is Frame;
            }, "BlueprintLabelExtra".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Blueprint || t is Frame || t.Map.designationManager.DesignationOn(t) != null;
            }, "P_Designations".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Building || t is Hive;
            }, "P_Building".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Pawn;
            }, "PawnsTabShort".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t.def.EverHaulable;
            }, "P_Items".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return true;
            }, "ShowAll".Translate()));
            CacheMenuOptions();
        }

        private static void CacheMenuOptions()
        {
            foreach (var v in Conditions)
            {
                CachedOptions.Add(new FloatMenuOption(v.label, delegate ()
                {
                    MainMod.ThingShowCond = v.Cond;
                }));
            }
        }

        public static void ShowConditionsMenuBox()
        {
            Find.WindowStack.Add(new FloatMenu(CachedOptions));
        }
    }
}
