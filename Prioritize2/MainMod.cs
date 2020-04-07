using System;
using RimWorld;
using Verse;
using HugsLib;

namespace Prioritize2
{
    public class MainMod : ModBase
    {
        public static ModSettings ModConfig;

        public static PriorityData Data;

        public static MainMod Instance;

        public override string ModIdentifier => "Priortize2";
    }
}
