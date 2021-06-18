using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class BannedWorkGiverEntry : IExposable
    {
        public string typeName;
        public string description;
        public string modId;
        public bool enabled;

        public BannedWorkGiverEntry()
        {

        }

        public bool IsEnabled()
        {
            return enabled && ModLister.GetActiveModWithIdentifier(modId) != null;
        }

        public Type GetTypeIfEnabled()
        {
            if (IsEnabled())
            {
                return GenTypes.GetTypeInAnyAssembly(typeName);
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("BannedWorkGiverEntry");
            builder.AppendLine("typeName=" + typeName);
            builder.AppendLine("type=" + GetTypeIfEnabled());
            builder.AppendLine("description=" + description);
            builder.AppendLine("modId=" + modId);
            builder.AppendLine("found modId=" + (ModLister.GetActiveModWithIdentifier(modId) != null).ToString());
            builder.AppendLine("enabled=" + enabled.ToString());
            builder.AppendLine("IsEnabled=" + IsEnabled().ToString());

            return builder.ToString();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeName, "typeName");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref modId, "modId");
            Scribe_Values.Look(ref enabled, "enabled");
        }

        public static BannedWorkGiverEntry GetBannedWorkGiverEntry(string typeName, string description, string modId, bool enabled)
        {
            var result = new BannedWorkGiverEntry();

            result.typeName = typeName;
            result.description = description;
            result.modId = modId;
            result.enabled = enabled;

            return result;
        }

        public static IEnumerable<BannedWorkGiverEntry> GetDefaultEntries()
        {
            yield return GetBannedWorkGiverEntry(typeof(WorkGiver_RemoveRoof).FullName, "P2_BannedRemoveRoof".Translate(), "Ludeon.RimWorld", true);
            yield return GetBannedWorkGiverEntry(typeof(WorkGiver_ConstructFinishFrames).FullName, "P2_CompatSmartConstruction".Translate(), "dhultgren.smarterconstruction", false);
        }
    }
}
