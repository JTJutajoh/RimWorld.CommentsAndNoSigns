using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Dark.Signs
{
    [HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
    public class GenConstruct_CanPlaceBlueprintAt_Patch
    {
        static void Postfix(ref AcceptanceReport __result, BuildableDef entDef)
        {
            if (entDef == null)
            {
                return;
            }
            if (__result.Accepted == true)
            {
                return; // Already true, don't bother changing anything
            }
            if (__result.Reason == "CannotPlaceInUndiscovered".Translate())
            {
                if (Comp_Sign.BuildableCanGoOverFog(entDef))
                {
                    __result = true;
                }
            }
            else if (__result.Reason == "SpaceAlreadyOccupied".Translate())
            {
                if (Comp_Sign.BuildableCanGoOverFog(entDef))
                {
                    __result = true;
                }
            }
        }
    }
}
