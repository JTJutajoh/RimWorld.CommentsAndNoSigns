using System.Collections.Generic;
using Verse;

namespace Dark.Signs
{
    public static class SignUtils
    {
        public static void UpdateSignsOnRoomChange(Room room)
        {
            List<Thing> containedThings = room.ContainedAndAdjacentThings;
            //List<Thing> containedThings = __instance.ContainedThings();
            //foreach (Thing thing in containedThings)
            
            for (int i = 0; i < containedThings.Count; i++)
            {
                //Building_Sign_RoomSign building_roomSign = containedThings[i] as Building_Sign_RoomSign;
                Thing building_roomSign = containedThings[i];
                if (building_roomSign != null)
                {
                    Comp_Sign signComp = building_roomSign.TryGetComp<Comp_Sign>();
                    if (signComp != null)
                    {
                        if (signComp.isRoomSign)
                            signComp.SetContentsFromRoom();
                    }
                }
            }
        }
    }
}
