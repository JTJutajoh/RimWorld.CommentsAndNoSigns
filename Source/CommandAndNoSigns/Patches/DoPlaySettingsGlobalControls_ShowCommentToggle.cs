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
    public class DoPlaySettingsGlobalControls_ShowCommentToggle
    {
        public static bool drawComments = true;
        private static bool lastVal = drawComments;
        private static List<Comp_Sign> NotifyList;
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

            row.ToggleableIcon(ref drawComments, ContentFinder<Texture2D>.Get("UI/CommentUI", true), "Signs_ToggleToolTip".Translate(), SoundDefOf.Mouseover_ButtonToggle);
            if (drawComments != lastVal)
            {
                RefreshAllSigns();

                lastVal = drawComments;
            }
        }

        public static void RefreshAllSigns()
        {
            if (NotifyList == null || NotifyList.Count <= 0) return;
            foreach (Comp_Sign sign in NotifyList)
            {
                sign?.NotifyVisibilityChange();
            }
        }
        public static void RegisterForNotify(Comp_Sign inst)
        {
            if (NotifyList == null)
            {
                NotifyList = new List<Comp_Sign>();
            }
            NotifyList.Add(inst);
        }
    }
}
