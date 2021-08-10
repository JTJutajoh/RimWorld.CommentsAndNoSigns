using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace Dark.Signs
{
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public class ShowCommentToggle
    {
        public static bool drawComments = true;
        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
            {
                return;
            }

            if (Settings.addCommentToggle == false)
            {
                return; // Button disabled in settings
            }

            row.ToggleableIcon(ref drawComments, ContentFinder<Texture2D>.Get("UI/CommentUI", true), "Toggle visibility of sign labels", SoundDefOf.Mouseover_ButtonToggle);
        }
    }
}
