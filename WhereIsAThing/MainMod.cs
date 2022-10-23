using System;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld.Planet;

namespace ItemListSelector
{
    public class MainMod : Mod
    {
        public static MainMod instance;

        public MainMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("InsertKey.ItemListSelector");
            harmony.PatchAll();
            Compat_ToggleableReadouts.PatchIfNeeded(harmony);
            /*
            if (ModLister.GetActiveModWithIdentifier("CodeOptimist.CustomThingFilters") != null)
            {
                Compat.Compat_CustomThingFilters.Patch(HarmonyInst);
            }
            */
            instance = this;
        }

        private ThingDef toSelect;

        public void WillSelectThisInStorage(ThingDef thingDef)
        {
            toSelect = thingDef;
        }

        public void DoIfNotConsumed()
        {
            if (Event.current.type != EventType.Used && toSelect != null)
            {
                Event.current.Use();
                SelectThisInStorage(toSelect);
            }
            toSelect = null;
        }

        /// <summary>
        /// Tries to select that thingDef in CurrentMap if the event is not consumed.
        /// </summary>
        /// <param name="thingDef"></param>
        private static void SelectThisInStorage(ThingDef thingDef)
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
            var thingsGroupsList = Find.CurrentMap.haulDestinationManager.AllHaulDestinationsListForReading;

            for (int i = 0; i < thingsGroupsList.Count; i++)
            {
                var sg = thingsGroupsList[i];
                if (sg is ISlotGroupParent)
                {
                    ((ISlotGroupParent)sg).GetSlotGroup().HeldThings.DoIf(predicate, (Thing t) => s.Select(t));
                }
                else if (sg is Thing)
                {
                    ThingOwner owner = ((Thing)sg).TryGetInnerInteractableThingOwner();
                    if (owner != null) owner.DoIf(predicate, (Thing t) => s.Select(t));
                }
            }
            s.dragBox.active = false;
        }
    }
}
