using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using ColourPicker;

namespace Dark.Signs
{
    class Settings : Verse.ModSettings
    {
        static public bool useCharacterLimit = true;
        static public int characterLimit = 512;
        static private string charLimEditBuffer = characterLimit.ToString();
        static public bool hideLabelsWhenZoomedOut = false;
        static public bool alwaysShowLabels = true;
        static public bool addCommentToggle = true;
        static public int minZoomLevel = (int)CameraZoomRange.Middle;
        static public int maxZoomLevel = (int)CameraZoomRange.Middle;
        static public float worldVerticalOffset = 0.5f;
        static public Color globalLabelColor = GenMapUI.DefaultThingLabelColor.ToOpaque();


        public static Settings Get()
        {
            return LoadedModManager.GetMod<Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.ColumnWidth = inRect.width / 2.2f;

            listingStandard.Label("Signs_SettingsDisplay".Translate());
            listingStandard.GapLine();

            // Offset
            listingStandard.Label("Signs_SettingsYOffset".Translate() + " " + (worldVerticalOffset-0.5f).ToString("N2"));
            worldVerticalOffset = listingStandard.Slider(worldVerticalOffset, -1f, 1f);

            listingStandard.GapLine();

            // Color
            StringBuilder col = new StringBuilder();
            col.Append("(");
            col.Append((int)(globalLabelColor.r * 100));
            col.Append(",");
            col.Append((int)(globalLabelColor.g * 100));
            col.Append(",");
            col.Append((int)(globalLabelColor.b * 100));
            col.Append(",");
            col.Append((int)(globalLabelColor.a * 100));
            col.Append(")");
            /*listingStandard.Label("Signs_SettingsColor".Translate()+col+":");
            globalLabelColor.r = listingStandard.Slider(globalLabelColor.r, 0f, 1f);
            globalLabelColor.g = listingStandard.Slider(globalLabelColor.g, 0f, 1f);
            globalLabelColor.b = listingStandard.Slider(globalLabelColor.b, 0f, 1f);*/
            Rect colSettingRect = listingStandard.Label("Signs_SettingsColor".Translate() + ": " + col);
            colSettingRect.x += colSettingRect.width - 32f;
            colSettingRect.size = new Vector2(32f, 32f);
            Widgets.DrawBoxSolid(colSettingRect, Settings.globalLabelColor);
            if (Widgets.ButtonInvisible(colSettingRect, true))
            {
                Find.WindowStack.Add(new Dialog_ColourPicker(Settings.globalLabelColor,
                (newColor) =>
                {
                    Settings.globalLabelColor = newColor;
                }
                ) );
            }
            listingStandard.Gap();


            listingStandard.GapLine();

            // Hide settings
            listingStandard.CheckboxLabeled("Signs_SettingsAllZooms".Translate(), ref alwaysShowLabels, "Signs_SettingsAllZooms_desc".Translate());
            if (!alwaysShowLabels)
            {
                listingStandard.Gap();
                listingStandard.Indent();
                if (listingStandard.RadioButton("Signs_SettingsHideOut".Translate(), active: hideLabelsWhenZoomedOut, tooltip : "Signs_SettingsHideOut_desc".Translate()))
                    hideLabelsWhenZoomedOut = true;
                if (hideLabelsWhenZoomedOut)
                {
                    // Hide when zoomed OUT
                    listingStandard.Label("Signs_SettingsMinZoom".Translate() + ": " + ((CameraZoomRange)minZoomLevel).ToString() + " (" + minZoomLevel.ToString() + ")");
                    minZoomLevel = (int)listingStandard.Slider(minZoomLevel, (int)CameraZoomRange.Closest, (int)CameraZoomRange.Furthest);
                }

                if (listingStandard.RadioButton("Signs_SettingsHideIn".Translate(), active: !hideLabelsWhenZoomedOut, tooltip: "Signs_SettingsHideIn_desc".Translate()))
                    hideLabelsWhenZoomedOut = false;
                //listingStandard.CheckboxLabeled("Hide sign labels when zoomed out", ref hideLabelsWhenZoomedOut, "Hides all sign labels when the camera is zoomed out to help reduce visual clutter");
                if (!hideLabelsWhenZoomedOut)
                {
                    // Hide when zoomed IN
                    listingStandard.Label("Signs_SettingsMaxZoom".Translate() + ": " + ((CameraZoomRange)maxZoomLevel).ToString() + " (" + maxZoomLevel.ToString() + ")");
                    maxZoomLevel = (int)listingStandard.Slider(maxZoomLevel, (int)CameraZoomRange.Closest, (int)CameraZoomRange.Furthest);
                }
            }

            listingStandard.NewColumn();


            // Toggle button
            listingStandard.CheckboxLabeled("Signs_SettingsAddToggle".Translate(), ref addCommentToggle, "Signs_SettingsAddToggle_desc".Translate());

            listingStandard.GapLine();

            listingStandard.CheckboxLabeled("Signs_SettingsUseCharLimit".Translate(), ref useCharacterLimit, "Signs_SettingsUseCharLimit_desc".Translate());
            if (!useCharacterLimit)
            {
                Rect warningRect = listingStandard.Label("Signs_SettingsUseCharLimitWarning".Translate());
                Color colred = Color.red;
                colred.a = 0.5f;
                Widgets.DrawBoxSolid(warningRect, colred);
            }
            else
            {
                listingStandard.Indent(32);
                listingStandard.IntEntry(ref characterLimit, ref charLimEditBuffer);
                listingStandard.Outdent(32);
            }

            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hideLabelsWhenZoomedOut, "hideLabelsWhenZoomedOut", false);
            Scribe_Values.Look(ref alwaysShowLabels, "alwaysShowLabels", true);
            Scribe_Values.Look(ref addCommentToggle, "addCommentToggle", true);
            Scribe_Values.Look(ref minZoomLevel, "minZoomLevel", (int)CameraZoomRange.Middle);
            Scribe_Values.Look(ref maxZoomLevel, "maxZoomLevel", (int)CameraZoomRange.Middle);
            Scribe_Values.Look(ref worldVerticalOffset, "worldVerticalOffset", 0.5f);
            Scribe_Values.Look(ref globalLabelColor, "globalLabalColor", GenMapUI.DefaultThingLabelColor);
        }
    }
}
