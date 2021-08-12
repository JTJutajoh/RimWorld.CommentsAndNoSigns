using System;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Dark.Signs
{
    // Allows comments to be selected no matter what, even if they are over fog.
    [HarmonyPatch(typeof(ThingSelectionUtility), "SelectableByMapClick")]
    public class ThingSelectionUtility_SelectableByMapClick_Patch
    {
        static void Postfix(ref bool __result, Thing t)
        {
            if (Comp_Sign.ShouldDrawThingOverFog(t.def))
            {
                __result = true;
            }
        }
    }
}
