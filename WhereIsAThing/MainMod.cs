using System;
using HugsLib;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Utils;
using System.Reflection;
using System.Collections.Generic;
using RimWorld.Planet;

namespace ItemListSelector
{
    public class MainMod : ModBase
    {
        public override string ModIdentifier => "ItemListSelector";

        public static ModLogger logger;

        public static List<string> t = new List<string>();
        public static CategorizedOpenSave Save;

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            logger = Logger;
            foreach(string s in t)
            {
                Logger.Message(s);
            }
            
            /*
            if (ModLister.GetActiveModWithIdentifier("CodeOptimist.CustomThingFilters") != null)
            {
                Compat.Compat_CustomThingFilters.Patch(HarmonyInst);
            }
            */
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            Save = Current.Game.GetComponent<CategorizedOpenSave>();
            Save.ExpandCategories();
        }

        public static void SelectThisInStorage(ThingDef thingDef, Map map)
        {
            Selector s = Find.Selector;
            if (s == null) return;

            if (!Input.GetKey(KeyCode.LeftShift)) s.ClearSelection();
            Func<Thing, bool> predicate = delegate (Thing t)
            {
                if (t == null) return false;
                if (t.def != thingDef || s.IsSelected(t))
                {
                    return false;
                }
                return true;
            };
            var thingsGroupsList = Find.CurrentMap.haulDestinationManager.AllGroupsListForReading;

            for (int i = 0; i < thingsGroupsList.Count; i++)
            {
                var sg = thingsGroupsList[i];
                sg.HeldThings.DoIf(predicate, (Thing t) => s.Select(t));
            }
            s.dragBox.active = false;
        }
    }
}
