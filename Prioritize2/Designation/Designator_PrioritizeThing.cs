using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2.Designation
{
    public class Designator_PrioritizeThing : Designator
    {
        public override int DraggableDimensions => 2;

        protected override DesignationDef Designation => PrioritizeDesignationDefOf.Priortize_Thing;

        public Designator_PrioritizeThing()
        {
            defaultLabel = "P2_PrioritizeThing".Translate();
            defaultDesc = "P2_PrioritizeThingDesc".Translate();
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = false;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.InBounds(Map))
            {
                return false;
            }
            if (loc.GetThingList(Map).FirstOrDefault((Thing t) => CanDesignateThing(t).Accepted) == null)
            {
                return false;
            }
            return true;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            //Can't prioritize other faction's things
            if (t.Faction?.IsPlayer != true) return false;

            return MainMod.Data.Filter.Allows(t) && PriorityData.CanPrioritize(t);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (var thing in c.GetThingList(Map))
            {
                if (CanDesignateThing(thing).Accepted) DesignateThing(thing);
            }
        }

        public override void DesignateThing(Thing t)
        {
            MainMod.Data.SetPriority(t, MainMod.SelectedPriority);
        }
    }
}
