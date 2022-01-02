using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Prioritize2
{
    [StaticConstructorOnStartup]
    public class PriorityRender
    {
        private bool anyDirty = false;

        private bool markedDraw = false;

        public List<Thing> toDrawLabels = null;

        //If null, mark all maps dirty
        public void MarkDirty(Map map)
        {
            if (map == null)
            {
                foreach (var m in Find.Maps)
                {
                    m.GetPriorityData().RenderCache.IsDirty = true;
                }
            }
            else
            {
                map.GetPriorityData().RenderCache.IsDirty = true;
            }
            anyDirty = true;
            Log.Message($"Mark dirty called with map={map}");
        }

        public void Recalculate()
        {
            Log.Message("[Prioritize2] PriorityRender.Recalculate() called");
            if (!anyDirty)
            {
                Log.Warning("Recalculate called with AnyDirty = false");
            }

            LongEventHandler.QueueLongEvent(
                RecalculateInternal, 
                "P2_Recalculate".Translate(), 
                false, 
                delegate (Exception e) 
                { 
                    Log.Error("Exception while recalculating priority render : " + e);
                },
                false);

            anyDirty = false;
        }

        private void RecalculateInternal()
        {
            var dirtymaps = from map in Find.Maps where map.GetPriorityData().RenderCache.IsDirty select map;
            int count = dirtymaps.Count();

            foreach (var map in dirtymaps)
            {
                var priCache = map.GetPriorityData().RenderCache;

                priCache.ThingCache.Clear();
                priCache.IsDirty = false;

                foreach (var thing in map.listerThings.AllThings)
                {
                    if (MainMod.Data.ViewFilter.Allows(thing) && MainMod.Data.HasPriority(thing))
                    {
                        priCache.ThingCache.Add(thing);
                    }
                }
            }

            if (count == 0)
            {
                Log.Warning("Recalculate called but there's no dirty map caches.");
            }
        }

        public void ThingPriorityUpdated(Thing t, int newPriority)
        {
            if (t.Map == null) return;

            var data = t.Map.GetPriorityData();

            if (newPriority == 0)
            {
                data.RenderCache.ThingCache.Remove(t);
            }
            else
            {
                bool matchesFilter = MainMod.Data.ViewFilter.Allows(t);

                if (matchesFilter)
                {
                    data.RenderCache.ThingCache.Add(t);
                }
                else
                {
                    Log.Message("This shouldn't called?");
                    data.RenderCache.ThingCache.Remove(t);
                }
            }
        }

        public void MarkDraw()
        {
            markedDraw = true;
        }

        public void Update()
        {
            if (Find.Maps.NullOrEmpty()) return;

            //If Priority Marks is being rendered and AnyDirty
            if (markedDraw && anyDirty)
            {
                Recalculate();
            }

            //If Priority Marks are being rendered
            if (markedDraw)
            {
                RenderPriorityMarks();
                markedDraw = false;
            }
            else
            {
                toDrawLabels = null;
            }
        }

        public void OnGUI()
        {
            if (toDrawLabels != null)
            {
                var list = toDrawLabels;

                foreach (Thing t in list)
                {
                    int pri = MainMod.Data.GetPriority(t);

                    GenMapUI.DrawThingLabel(t, pri.ToString(), pri.GetPriorityColor());
                }
            }
        }

        public void RenderPriorityMarks()
        {
            if (Find.CurrentMap == null) return;

            Map map = Find.CurrentMap;

            IntVec3 mouseAt = UI.MouseCell();

            CellRect cameraRect = Find.CameraDriver.CurrentViewRect;
            cameraRect.ClipInsideMap(map);
            cameraRect = cameraRect.ExpandedBy(1);

            List<Thing> newDrawLabels = new List<Thing>();

            foreach (Thing t in map.GetPriorityData().RenderCache.ThingCache)
            {
                if (t.DestroyedOrNull()) continue;

                IntVec3 pos = t.Position;

                if (cameraRect.Contains(pos))
                {
                    DrawPriorityMarkTo(t);

                    if (mouseAt.DistanceTo(pos) < 10f)
                    {
                        newDrawLabels.Add(t);
                    }
                }
            }

            toDrawLabels = newDrawLabels;
        }

        public static readonly Material PriorityThingOverlayMat = MaterialPool.MatFrom("Prioritize2/UI/PriorityThingOverlay", ShaderDatabase.MetaOverlay);

        protected MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private void DrawPriorityMarkTo(Thing t)
        {
            int priority = MainMod.Data.GetPriority(t);
            Color color = priority.GetPriorityColor();

            propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10, t.DrawPos, Quaternion.identity, PriorityThingOverlayMat, 0, null, 0, propertyBlock);
        }
    }
}
