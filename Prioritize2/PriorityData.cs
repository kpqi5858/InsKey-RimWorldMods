using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityData : GameComponent
    {
        //Key : Thing.thingIDNumber, Value : Priority
        private Dictionary<int, int> ThingPriority = new Dictionary<int, int>();
        //Key : Map.uniqueID, Value : PriorityMapGrid
        private Dictionary<int, PriorityMapGrid> GridPriority = new Dictionary<int, PriorityMapGrid>();

        public PriorityRender Render;

        private PriorityFilter FilterInternal;

        public PriorityFilter Filter
        {
            get
            {
                return FilterInternal;
            }
            set
            {
                FilterInternal = value;

                Render.MarkDirty(null);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref ThingPriority, "thingPriority", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref GridPriority, "gridPriority", LookMode.Value, LookMode.Deep);
            Scribe_Deep.Look(ref FilterInternal, "priorityFilter");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            RemoveInvalids();
            Render.MarkDirty(null);
        }

        public void RemoveInvalids()
        {
            Dictionary<int, int> NewThingPri = new Dictionary<int, int>();
            foreach (var map in Find.Maps)
            {
                var things = map.listerThings.AllThings;
                for (int i = 0; i < things.Count; i++)
                {
                    var thing = things[i];
                    if (thing.def.HasThingIDNumber && ThingPriority.TryGetValue(thing.thingIDNumber, out int pri))
                    {
                        NewThingPri.Add(thing.thingIDNumber, pri);
                    }
                }
            }

            Log.Message("Removed " + (ThingPriority.Count - NewThingPri.Count) + " invalid things in ThingPriority.");
            ThingPriority = NewThingPri;
        }

        public void Notify_MapRemoved(Map map)
        {
            GridPriority.Remove(map.uniqueID);
            Render.Notify_MapRemoved(map);
        }

        public void ThingDestroyed(Thing thing)
        {
            //Remove priority on it
            if (IsValid(thing)) SetPriority(thing, 0);
        }

        public PriorityMapGrid GetOrCreatePriorityMapGrid(Map map)
        {
            if (GridPriority.TryGetValue(map.uniqueID, out PriorityMapGrid grid))
            {
                return grid;
            }

            var newGrid = new PriorityMapGrid(map);
            GridPriority.Add(map.uniqueID, newGrid);

            return newGrid;
        }

        //Can assign a priority to it?
        private bool IsValidDoError(Thing thing, string methodName)
        {
            if (thing == null)
            {
                Log.ErrorOnce(methodName + " with null thing.", ("P2_1" + methodName).GetHashCode());
                return false;
            }
            if (!thing.def.HasThingIDNumber)
            {
                Log.ErrorOnce(methodName + " with thing that has no thingIDNumber.", ("P2_2" + methodName).GetHashCode());
                return false;
            }
            return true;
        }

        private bool IsValid(Thing thing)
        {
            return thing != null && thing.def.HasThingIDNumber;
        }

        public int GetPriority(Thing thing)
        {
            if (!IsValidDoError(thing, "GetPriority")) return 0;

            //GenCollection.TryGetValue - Returns fallback one if not found in dictionary
            return ThingPriority.TryGetValue(thing.thingIDNumber, 0);
        }

        public bool HasPriority(Thing thing)
        {
            if (!IsValidDoError(thing, "HasPriority")) return false;

            return ThingPriority.ContainsKey(thing.thingIDNumber);
        }
        
        //If priority is 0, removes priority
        public void SetPriority(Thing thing, int priority)
        {
            if (!IsValidDoError(thing, "SetPriority")) return;

            if (priority == 0)
            {
                ThingPriority.Remove(thing.thingIDNumber);
            }
            else
            {
                ThingPriority.Add(thing.thingIDNumber, priority);
            }
        }

        public int GetPriorityOnCell(Map map, IntVec3 cell)
        {
            if (map == null)
            {
                Log.ErrorOnce("GetPriorityOnCell with null map.", "P2_GPOC".GetHashCode());
                return 0;
            }
            var grid = GetOrCreatePriorityMapGrid(map);

            return grid.GetPriorityAt(cell);
        }

        public void SetPriorityOnCell(Map map, IntVec3 cell, int pri)
        {
            if (map == null)
            {
                Log.ErrorOnce("SetPriorityOnCell with null map.", "P2_SPOC".GetHashCode());
                return;
            }
            var grid = GetOrCreatePriorityMapGrid(map);

            grid.SetPriorityAt(cell, pri);
        }

    }
}
