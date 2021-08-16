using HarmonyLib;
using Verse;
using RimWorld;

namespace Dark.Signs
{
#if v1_3
    [HarmonyPatch(typeof(Room), "Notify_BedTypeChanged")]
    public class Room_Notify_BedTypeChanged_Patch
    {
        static void Postfix(ref Room __instance)
        {
            //Messages.Message("Notify_BedTypeChanged", MessageTypeDefOf.TaskCompletion, historical: false);
            SignUtils.UpdateSignsOnRoomChange(__instance);
        }
    }
#endif
}
