using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Prioritize
{
    public class Designator_PrioritySettings : Designator
    {
        public Designator_PrioritySettings()
        {
            icon = ContentFinder<Texture2D>.Get("UI/Prioritize/PrioritySettings", true);
            defaultLabel = "P_SelectionOptions".Translate();
            defaultDesc = "P_SelectionOptionsDesc".Translate();
        }

        public override void ProcessInput(Event ev)
        {
            if (CheckCanInteract())
            {
                PriorityShowConditions.ShowConditionsMenuBox();
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return false;
        }
    }
}
