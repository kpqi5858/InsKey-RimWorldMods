using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Prioritize2.Designation
{
    public class Designator_PrioritizeZone : Designator
    {
        public override int DraggableDimensions => 2;

        protected override DesignationDef Designation => PrioritizeDesignationDefOf.Priortize_Zone;

        public override bool DragDrawMeasurements => true;

        public Designator_PrioritizeZone()
        {
            defaultLabel = "P2_PrioritizeZone".Translate();
            defaultDesc = "P2_PrioritizeZoneDesc".Translate();
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_PlanAdd;
            icon = ContentFinder<Texture2D>.Get("Prioritize2/UI/ZonePriorityDesignator");
            useMouseIcon = false;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return loc.InBounds(Map);
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            if (loc.InBounds(Map))
            {
                MainMod.Data.SetPriorityOnCell(Map, loc, MainMod.SelectedPriority);
            }
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

        public override void SelectedUpdate()
        {
            Map.GetPriorityData().MarkDraw();
        }
    }
}
