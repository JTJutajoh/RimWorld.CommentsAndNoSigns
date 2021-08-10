using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Dark.Signs
{
    class Settings : Verse.ModSettings
    {
        static public bool hideLabelsWhenZoomedOut = false;
        static public bool alwaysShowLabels = false;
        static public bool addCommentToggle = true;
        static public int minZoomLevel = (int)CameraZoomRange.Middle;
        static public int maxZoomLevel = (int)CameraZoomRange.Middle;
        static public float worldVerticalOffset = 0.5f;
        static public Color globalLabelColor = GenMapUI.DefaultThingLabelColor;


        public static Settings Get()
        {
            return LoadedModManager.GetMod<Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.ColumnWidth = inRect.width / 2.2f;

            listingStandard.Label("Display settings");
            listingStandard.GapLine();

            // Offset
            listingStandard.Label("Label vertical offset: " + worldVerticalOffset.ToString("N2"));
            worldVerticalOffset = listingStandard.Slider(worldVerticalOffset, -1f, 1f);

            listingStandard.GapLine();

            // Color
            StringBuilder col = new StringBuilder();
            col.Append("(");
            col.Append(globalLabelColor.r.ToString("N1"));
            col.Append(",");
            col.Append(globalLabelColor.g.ToString("N1"));
            col.Append(",");
            col.Append(globalLabelColor.b.ToString("N1"));
            col.Append(")");
            listingStandard.Label("Label color"+col+":");
            globalLabelColor.r = listingStandard.Slider(globalLabelColor.r, 0f, 1f);
            globalLabelColor.g = listingStandard.Slider(globalLabelColor.g, 0f, 1f);
            globalLabelColor.b = listingStandard.Slider(globalLabelColor.b, 0f, 1f);

            listingStandard.GapLine();

            // Hide settings
            listingStandard.CheckboxLabeled("Show sign labels at all zooms", ref alwaysShowLabels, "Show sign labels at any zoom level. All sign labels can be toggled off and on at any time using the speech bubble button added to the bottom right of the in-game UI.");
            if (!alwaysShowLabels)
            {
                listingStandard.Gap();
                listingStandard.Indent();
                if (listingStandard.RadioButton("Hide sign labels when zoomed out", active: hideLabelsWhenZoomedOut, tooltip : "Only show sign labels when the camera is zoomed in closer than the below threshold"))
                    hideLabelsWhenZoomedOut = true;
                if (hideLabelsWhenZoomedOut)
                {
                    // Hide when zoomed OUT
                    listingStandard.Label("Minimum Zoom level: " + ((CameraZoomRange)minZoomLevel).ToString() + " (" + minZoomLevel.ToString() + ")");
                    minZoomLevel = (int)listingStandard.Slider(minZoomLevel, (int)CameraZoomRange.Closest, (int)CameraZoomRange.Furthest);
                }

                if (listingStandard.RadioButton("Hide sign labels when zoomed in", active: !hideLabelsWhenZoomedOut, tooltip: "Only show sign labels when the camera is zoomed out further than the below threshold"))
                    hideLabelsWhenZoomedOut = false;
                //listingStandard.CheckboxLabeled("Hide sign labels when zoomed out", ref hideLabelsWhenZoomedOut, "Hides all sign labels when the camera is zoomed out to help reduce visual clutter");
                if (!hideLabelsWhenZoomedOut)
                {
                    // Hide when zoomed IN
                    listingStandard.Label("Maximum Zoom level: " + ((CameraZoomRange)maxZoomLevel).ToString() + " (" + maxZoomLevel.ToString() + ")");
                    maxZoomLevel = (int)listingStandard.Slider(maxZoomLevel, (int)CameraZoomRange.Closest, (int)CameraZoomRange.Furthest);
                }
            }

            listingStandard.NewColumn();


            // Toggle button
            listingStandard.CheckboxLabeled("Add label toggle button", ref addCommentToggle, "Whether or not the button for quickly toggling all sign labels at once should be added to the UI in the bottom right. Turn this off if you want them to always be visible/invisible and your UI is cluttered.");

            listingStandard.Indent();
            listingStandard.Label("Example toggle button:");
            listingStandard.ButtonImage(ContentFinder<Texture2D>.Get("UI/CommentUI", true), 20, 20);
            listingStandard.Outdent();

            listingStandard.GapLine();

            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hideLabelsWhenZoomedOut, "hideLabelsWhenZoomedOut", false);
            Scribe_Values.Look(ref alwaysShowLabels, "alwaysShowLabels", false);
            Scribe_Values.Look(ref addCommentToggle, "addCommentToggle", true);
            Scribe_Values.Look(ref minZoomLevel, "minZoomLevel", (int)CameraZoomRange.Middle);
            Scribe_Values.Look(ref maxZoomLevel, "maxZoomLevel", (int)CameraZoomRange.Middle);
            Scribe_Values.Look(ref worldVerticalOffset, "worldVerticalOffset", 0.5f);
            Scribe_Values.Look(ref globalLabelColor, "globalLabalColor", GenMapUI.DefaultThingLabelColor);
        }
    }
}
