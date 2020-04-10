using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2.Designation
{
    public class Designator_PrioritizeZone : Designator
    {
        public override int DraggableDimensions => 2;

        protected override DesignationDef Designation => PrioritizeDesignationDefOf.Priortize_Zone;

        public Designator_PrioritizeZone()
        {
            defaultLabel = "P2_PrioritizeZone".Translate();
            defaultDesc = "P2_PrioritizeZoneDesc".Translate();
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = false;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return loc.InBounds(Map);
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            foreach (var loc in cells)
            {
                if (loc.InBounds(Map))
                {
                    MainMod.Data.SetPriorityOnCell(Map, loc, MainMod.SelectedPriority);
                }
            }
        }
    }
}
