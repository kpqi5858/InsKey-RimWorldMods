using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityFilter : IExposable
    {

        public bool Allows(Thing thing)
        {
            return false;
        }

        public void ExposeData()
        {
            
        }
    }
}
