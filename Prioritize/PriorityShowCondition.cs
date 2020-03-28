using System;
using Verse;

namespace Prioritize
{
    public struct PriorityShowCondition
    {
        public Func<Thing, bool> Cond;
        public string label;

        public PriorityShowCondition(Func<Thing, bool> cond, string lab)
        {
            Cond = cond;
            label = lab;
        }
    }
}
