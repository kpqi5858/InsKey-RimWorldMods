using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace Prioritize
{
    public class Designator_Priority_Cell : Designator_Priority_Thing
    {
        public Designator_Priority_Cell() : base()
        {
            defaultLabel = "P_DesignatorCellLabel".Translate();
            defaultDesc = "P_DesignatorCellDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Prioritize/CellPri", true);
        }
        public override bool DragDrawMeasurements => true;
        protected override DesignationDef Designation => PDefOf.Priortize_Cell;

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.InBounds(Map)) return false;
            return true;
        } 

        public override void DesignateSingleCell(IntVec3 c)
        {
            MainMod.save.GetOrCreatePriorityMapData(Map).SetPriorityAt(c, MainMod.SelectedPriority);
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }
    }
}
