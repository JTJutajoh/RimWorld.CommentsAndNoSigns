using HarmonyLib;
using Verse;
using RimWorld;

namespace Dark.Signs
{
#if v1_3 || v1_4
    [HarmonyPatch(typeof(Room), "Notify_RoomShapeChanged")]
    public class Room_Notify_RoomShapeChanged
    {
        static void Postfix(ref Room __instance)
        {
            //Messages.Message("Notify_RoomShapeChanged", MessageTypeDefOf.TaskCompletion, historical: false);
            SignUtils.UpdateSignsOnRoomChange(__instance);
        }
    }
#endif
}
