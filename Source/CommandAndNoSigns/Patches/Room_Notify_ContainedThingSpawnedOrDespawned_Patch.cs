using HarmonyLib;
using Verse;
using RimWorld;

namespace Dark.Signs
{
    
    // Disabled in lieu of ticking the signs since that still happens less often than this notify
    /*[HarmonyPatch(typeof(Room), "Notify_ContainedThingSpawnedOrDespawned")]
    public class Room_Notify_ContainedThingSpawnedOrDespawned
    {
        static void Postfix(ref Room __instance, Thing th)
        {
            ThingCategory thcat = th.def.category;
            if (thcat == ThingCategory.Mote || thcat == ThingCategory.Projectile || thcat == ThingCategory.Ethereal || thcat == ThingCategory.Pawn)
            {
                // Notify was for something we don't care about
                return;
            }
            //Messages.Message("Notify_ContainedThingSpawnedOrDespawned", MessageTypeDefOf.TaskCompletion, historical: false);
            SignUtils.UpdateSignsOnRoomChange(__instance);
        }
    }*/
}
