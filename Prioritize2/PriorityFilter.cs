using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityFilter : IExposable
    {
        public enum ThingSelection
        {
            None, //Can't select anything
            NeedsHauling, //Can select things need hauling
            Selectable, //Can select selectable thing
            Everything //Can select everything wtf
        }

        public ThingSelection generalFilter;

        public bool designationOn;
        public bool blueprints;

        public bool Allows(Thing thing)
        {
            //Essential checks
            if (!PriorityData.CanPrioritize(thing)) return false;
            if (!thing.Position.IsValid) return false;
            if (thing.Fogged()) return false;

            if (AllowsThingSelection(generalFilter, thing)) return true;

            if (designationOn && thing.Map.designationManager.DesignationOn(thing) != null) return true;
            if (designationOn && thing.Map.designationManager.HasMapDesignationAt(thing.Position)) return true;

            if (blueprints && (thing is Frame || thing is Blueprint)) return true;

            return false;
        }

        private bool AllowsThingSelection(ThingSelection selection, Thing thing)
        {
            if (selection == ThingSelection.None)
            {
                return false;
            }
            else if (selection == ThingSelection.NeedsHauling)
            {
                return thing.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Contains(thing);
            }
            else if (selection == ThingSelection.Selectable)
            {
                return ThingSelectionUtility.SelectableByMapClick(thing);
            }
            else if (selection == ThingSelection.Everything)
            {
                return true;
            }

            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref generalFilter, "generalFilter");
            Scribe_Values.Look(ref designationOn, "designationOn");
            Scribe_Values.Look(ref blueprints, "blueprints");
        }

        public static PriorityFilter GetDefaultFilter()
        {
            PriorityFilter filter = new PriorityFilter();

            filter.generalFilter = ThingSelection.None;
            filter.designationOn = true;
            filter.blueprints = true;

            return filter;
        }
    }
}
