using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Prioritize2.Designation;

namespace Prioritize2
{
    public class PriorityData : GameComponent
    {
        //Key : Thing.thingIDNumber, Value : Priority
        private Dictionary<int, int> ThingPriority = new Dictionary<int, int>();

        public PriorityRender Render = new PriorityRender();

        private PriorityFilter FilterInternal;

        public List<Thing> toDrawLabels = null;

        public List<Thing> toRemovePriority = new List<Thing>();

        public PriorityFilter Filter
        {
            get
            {
                if (FilterInternal == null)
                {
                    FilterInternal = PriorityFilter.GetDefaultFilter();
                }

                return FilterInternal;
            }
            set
            {
                FilterInternal = value;

                Render.MarkDirty(null);
            }
        }

        public PriorityData(Game game)
        {
            MainMod.Data = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref ThingPriority, "thingPriority", LookMode.Value, LookMode.Value);
            Scribe_Deep.Look(ref FilterInternal, "priorityFilter");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            RemoveInvalids();
            Render.MarkDirty(null);
        }

        public override void GameComponentUpdate()
        {
            Render.Update();
        }

        public override void GameComponentOnGUI()
        {
            if (toDrawLabels != null)
            {
                var list = toDrawLabels;

                foreach (Thing t in list)
                {
                    int pri = GetPriority(t);

                    GenMapUI.DrawThingLabel(t, pri.ToString(), pri.GetPriorityColor());
                }
            }

            MousePriorityControl();
        }

        public override void GameComponentTick()
        {
            RemoveDestroyedNow();
        }

        //Ctrl+Wheel to select priority
        private void MousePriorityControl()
        {
            var selDesign = Find.DesignatorManager?.SelectedDesignator;
            bool shouldDo = selDesign is Designator_PrioritizeThing
                         || selDesign is Designator_PrioritizeZone;

            if (shouldDo)
            {
                if (Event.current.type == EventType.ScrollWheel && Input.GetKey(KeyCode.LeftControl))
                {
                    Event.current.Use();
                    MainMod.SelectedPriority -= Math.Sign(Event.current.delta.y);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);

                    MainMod.SelectedPriority.ClampPriority();

                    Vector2 mousePosition = Event.current.mousePosition;
                    Rect textRect = new Rect(mousePosition.x + 24f, mousePosition.y + 24f, 200f, 9999f);

                    Find.WindowStack.ImmediateWindow("P2_DrawPriority".GetHashCode(), textRect, WindowLayer.GameUI, delegate() 
                    {
                        GameFont font = Text.Font;
                        Text.Font = GameFont.Small;
                        Widgets.Label(textRect.AtZero(), MainMod.SelectedPriority.ToString());
                        Text.Font = font;
                    }
                    , false, false, 0f);
                }
            }
        }

        public void RemoveInvalids()
        {
            Dictionary<int, int> NewThingPri = new Dictionary<int, int>();
            foreach (var map in Find.Maps)
            {
                foreach (var thing in map.listerThings.AllThings)
                {
                    if (CanPrioritize(thing) && ThingPriority.TryGetValue(thing.thingIDNumber, out int pri))
                    {
                        NewThingPri.Add(thing.thingIDNumber, pri);
                    }
                }
            }

            Log.Message("Removed " + (ThingPriority.Count - NewThingPri.Count) + " invalid things in ThingPriority.");
            ThingPriority = NewThingPri;
        }

        public void ThingDestroyed(Thing thing)
        {
            //Remove priority on it
            if (CanPrioritize(thing) && HasPriority(thing))
            {
                toRemovePriority.Add(thing);

                if (toRemovePriority.Count > 10000)
                {
                    Log.Warning("Too many things in toRemovePriority list. Removing..");
                    RemoveDestroyedNow();
                }
            }
        }

        public void RemoveDestroyedNow()
        {
            foreach (Thing t in toRemovePriority)
            {
                bool result = ThingPriority.Remove(t.thingIDNumber);

                if (result == false)
                {
                    Log.ErrorOnce("Thing " + t + " was in toRemovePriority list, but not in ThingPriority.", "P2_TRP".GetHashCode());
                }
            }

            toRemovePriority.Clear();
        }

        //Can assign a priority to it?
        private bool CanPrioritizeDoErr(Thing thing, string methodName)
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

        public static bool CanPrioritize(Thing thing)
        {
            return thing != null && thing.def.HasThingIDNumber;
        }

        public int GetPriority(Thing thing)
        {
            if (!CanPrioritizeDoErr(thing, "GetPriority")) return 0;

            //GenCollection.TryGetValue - Returns fallback one if not found in dictionary
            return ThingPriority.TryGetValue(thing.thingIDNumber, 0);
        }

        public bool HasPriority(Thing thing)
        {
            if (!CanPrioritizeDoErr(thing, "HasPriority")) return false;

            return ThingPriority.ContainsKey(thing.thingIDNumber);
        }
        
        //If priority is 0, removes priority
        public void SetPriority(Thing thing, int priority)
        {
            if (!CanPrioritizeDoErr(thing, "SetPriority")) return;

            if (priority == 0)
            {
                ThingPriority.Remove(thing.thingIDNumber);
            }
            else
            {
                ThingPriority[thing.thingIDNumber] = priority;
            }

            Render.ThingPriorityUpdated(thing, priority);
        }

        public int GetPriorityOnCell(Map map, IntVec3 cell)
        {
            if (map == null)
            {
                Log.ErrorOnce("GetPriorityOnCell with null map.", "P2_GPOC".GetHashCode());
                return 0;
            }

            return map.GetPriorityData().GetPriorityAt(cell);
        }

        public void SetPriorityOnCell(Map map, IntVec3 cell, int pri)
        {
            if (map == null)
            {
                Log.ErrorOnce("SetPriorityOnCell with null map.", "P2_SPOC".GetHashCode());
                return;
            }

            map.GetPriorityData().SetPriorityAt(cell, pri);
        }

    }
}
