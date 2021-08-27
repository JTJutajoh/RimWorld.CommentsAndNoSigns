using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using DarkColourPicker_Forked;
using Epitaph;

namespace Dark.Signs
{
    public class Comp_Sign : ThingComp
    {
        public CompProperties_Sign Props => (CompProperties_Sign)this.props;

        public bool canEditContent => Props.canEditContent;
        public bool editOnPlacement => Props.editOnPlacement;
        public bool isRoomSign => Props.isRoomSign;

        // For compatibility with Epitaphs mod
        public static Assembly EpitaphAssem;
        private static bool EpitaphsLoaded = false;
        private bool hasEpitaph = false; // If this particular Thing has an epitaph comp (Graves/sarcophagi)
        private static PropertyInfo EpitaphInscriptionProperty; // Used to access the inscription from the epitaph comp
        private static Type EpitaphCompType; // Used to check if the Thing has an epitaph comp
        private object EpitaphComp; // For passing to the epitaph rename dialog


        public bool hideLabelOverride = false;
        private Graphic HiddenGraphic;
        public Graphic CurrentGraphic
        {
            get
            {
                // Swap out the graphic if comment graphics are set to be hidden currently
                if (Settings.commentToggleHidesComments && DoPlaySettingsGlobalControls_ShowCommentToggle.drawComments == false && this.parent.def.defName == "Comment")
                {
                    if (this.HiddenGraphic == null)
                    {
                        // Load the alternate graphic 
                        this.HiddenGraphic = GraphicDatabase.Get(this.parent.def.graphicData.graphicClass, this.parent.def.graphicData.texPath + "_Hidden",
                            this.parent.def.graphicData.shaderType.Shader, this.parent.def.graphicData.drawSize, this.parent.DrawColor, this.parent.DrawColorTwo, null);
                    }
                    return this.HiddenGraphic;
                }
                return this.parent.DefaultGraphic;
            }
        }

        public void NotifyVisibilityChange()
        {
            // Notified usually by the toggle comment button, possibly by the Settings window
            this.parent.DirtyMapMesh(this.parent.Map);
        }

        // init to null so we can check on spawn if there's any existing content
        private string _signContent;
        public string signContent
        {
            get
            {
                if (hasEpitaph)
                {
                    return (string)EpitaphInscriptionProperty.GetValue(EpitaphComp);
                }
                else
                    return _signContent;
            }
            set
            {
                _signContent = value;
            }
        }

        private GameFont _fontSize = GameFont.Tiny;
        public GameFont fontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
            }
        }

        private Color _labelColor = Settings.globalLabelColor;
        public Color labelColor
        {
            get
            {
                return this._labelColor;
            }
            set
            {
                _labelColor = value;
            }
        }

        public Comp_Sign() : base()
        {
            // Mod compat patches
            // Lazy load a reference to the Epitaphs assembly
            if (EpitaphAssem == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith("Epitaph"))
                    {
                        Log.Message("(Signs) Found Epitaph mod, patching");
                        EpitaphAssem = assembly;
                        EpitaphCompType = EpitaphAssem.GetType("Epitaph.Comp_Epitaph");
                        EpitaphInscriptionProperty = EpitaphCompType.GetProperty("Inscription");

                        EpitaphsLoaded = true;
                        break;
                    }
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (this.isRoomSign)
                {
                    this.SetContentsFromRoom();
                    //Messages.Message("Placed in room", MessageTypeDefOf.TaskCompletion, historical: false);
                    return;
                }

                if (signContent == null)
                {
                    signContent = Props.defaultContents;
                }
                

                if (this.editOnPlacement) // If the def says we should edit the comment as soon as it's placed
                {
                    Find.WindowStack.Add(new Dialog_RenameSign(this));
                }

                if (Comp_Sign.BuildableCanGoOverFog(this.parent.def) && this.parent.Fogged())
                {
                    //Messages.Message("Spawning dummy designation", MessageTypeDefOf.CautionInput, false);
                    DesignationManager designationManager = this.parent.Map.designationManager;
                    LocalTargetInfo target = new LocalTargetInfo(this.parent);
                    Designation dummy = new Designation(target, DefDatabase<DesignationDef>.GetNamed("CommentDummy"));
                    designationManager.AddDesignation(dummy);
                }
            }
            else
            {
                if (labelColor == null)
                {
                    Log.Error("Null label color loaded");
                }
                RemoveExtraLineEndings();
            }

            // Mod compat patches
            if (EpitaphsLoaded)
            {
                // Find the epitaph comp
                foreach (ThingComp comp in parent.AllComps)
                {
                    // Check if it is the same type we found in the constructor
                    if (EpitaphCompType.IsInstanceOfType(comp))
                    {
                        EpitaphComp = comp;
                        hasEpitaph = true;
                        break;
                    }
                }
            }

            if (this.parent.def.defName == "Comment")
            {
                DoPlaySettingsGlobalControls_ShowCommentToggle.RegisterForNotify(this);
            }
        }

        // Content utilities
        private void RemoveExtraLineEndings()
        {
            if (signContent == null)
            {
                return;
            }
            string orig = signContent.Trim();

            signContent = orig.Replace("\r", "");
        }

        public static string UnescapeContents(string raw)
        {
            string result = raw;
            result = result.Replace("&apos;", "'");
            result = result.Replace("&quot;", "\"");
            result = result.Replace("&gt;", ">");
            result = result.Replace("&lt;", "<");
            result = result.Replace("&amp;", "&");
            return result;
        }

        // Room sign methods
        public void SetContentsFromRoom()
        {
            Room room = this.parent.GetRoom();
            if (room != null)
            {
                string roomLabel = GetRoomRoleLabel(room).CapitalizeFirst();
                
                signContent = roomLabel;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            // Only use the tick if we're a room sign for performance reasons. Most other signs shouldn't be set to tick at all anyway
            if (Props.isRoomSign)
            {
                // On rare ticks, force an update to check if the room has changed and wasn't caught by a notify.
                //this isn't very efficient, but it should always work and performance SHOULDN'T hurt unless you spam these signs everywhere.
                SetContentsFromRoom(); // THIS MIGHT KILL PERFORMANCE with many signs on the map at once
            }
        }


        private bool ShouldDrawLabel()
        {
            if (Find.Selector.IsSelected(this.parent))
                return true;

            // True if this specific sign has been disabled by the user
            if (hideLabelOverride == true)
                return false;

            // Check if the button in the bottom right is toggled on
            if (DoPlaySettingsGlobalControls_ShowCommentToggle.drawComments == false)
                return false;

            // Check if we should ignore zoom level
            if (Settings.alwaysShowLabels)
                return true;

            // Check if we should hide when zoomed out
            if (Settings.hideLabelsWhenZoomedOut)
            {
                if ((int)Find.CameraDriver.CurrentZoom > Settings.minZoomLevel)
                    return false; // return early to hide the label
            }
            // Check if we should hide when zoomed IN
            else
            {
                if ((int)Find.CameraDriver.CurrentZoom < Settings.maxZoomLevel)
                    return false; // return early to hide the label
            }

            // Draw the label if none of the checks failed
            return true;
        }

        public override void DrawGUIOverlay()
        {
            if (ShouldDrawLabel())
            {
                DrawSignLabels();
            }
        }

        // Copied directly from EnvironmentStatsDrawer in Core
        protected static string GetRoomRoleLabel(Room room)
        {
            // Copied from Core
            Pawn pawn = null;
            Pawn pawn2 = null;
            //List<Pawn> ownersList = room.Owners.ToList(); // Modified from Core. Maybe revert this
            //foreach (Pawn pawn3 in ownersList)
            foreach (Pawn pawn3 in room.Owners)
            {
                if (pawn == null)
                {
                    pawn = pawn3;
                }
                else
                {
                    pawn2 = pawn3;
                }
            }
            TaggedString taggedString;
            if (pawn == null)
            {
#if v1_3
                taggedString = room.Role.PostProcessedLabelCap;
#elif v1_2
                taggedString = room.Role.LabelCap;
#endif
            }
            else if (pawn2 == null)
            {
                taggedString = "SomeonesRoom".Translate(pawn.LabelShort, room.Role.label, pawn.Named("PAWN"));
            }
            else
            {
                taggedString = "CouplesRoom".Translate(pawn.LabelShort, pawn2.LabelShort, room.Role.label, pawn.Named("PAWN1"), pawn2.Named("PAWN2"));
            }
            return taggedString;
        }

        private IEnumerable<string> DoSignContents()
        {
            if (this.signContent == null)
            {
                yield break;
            }
            else
            {
                foreach (string s in this.signContent.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.None))
                {
                    
                    yield return s;
                }
                yield break;
            }
        }

        // Cycles through font sizes, looping back around
        private void ChangeSize()
        {
            fontSize += 1;
            if (fontSize > GameFont.Medium)
            {
                fontSize = GameFont.Tiny;
            }
        }

        // Returns a different size depending on the font being used
        // used for the background texture to make it fit better
        private float GetLineHeightBGFraction()
        {
            switch (this.fontSize)
            {
                case GameFont.Tiny:
                    return 0.6f;
                case GameFont.Small:
                    return 0.7f;
                case GameFont.Medium:
                    return 0.8f;
                default:
                    return 0.6f;
            }
        }
        private Vector2 GetLabelSizeFor(string s)
        {
            Vector2 size = new Vector2();
            //size = Text.CalcSize(s);
            GUIContent content = new GUIContent(s);
            size = Text.CurFontStyle.CalcSize(content);
            //size.y *= GetLineHeight();
            return size;
        }

        // returns the direction-specific offset for the current rotation if defined. If not, returns the generic labelOffset
        private Vector2 GetLabelOffset()
        {
            Rot4 rot = this.parent.Rotation;
            // switch statement won't work here because the Rot4 shortcuts aren't const i guess. whatever
            if (rot == Rot4.South)
            {
                return Props?.labelOffset_South ?? Props?.labelOffset ?? new Vector2();
            }
            else if (rot == Rot4.North)
            {
                return Props?.labelOffset_North ?? Props?.labelOffset ?? new Vector2();
            }
            else if (rot == Rot4.West)
            {
                return Props?.labelOffset_West ?? Props?.labelOffset ?? new Vector2();
            }
            else if (rot == Rot4.East)
            {
                return Props?.labelOffset_East ?? Props?.labelOffset ?? new Vector2();
            }
            // shouldn't be possible but maybe
            return new Vector2();
        }

        private Vector2 GetSignLabelPos()
        {
            /*IntVec3 signPos = this.parent.OccupiedRect().CenterCell;// this.parent.Position;
            //this.parent.OccupiedRect().CenterCell;
            // Copied from LabelDrawPosFor with slight modification
            // Adjusts the position to be the top of the cell instead of the center
            Vector3 position1 = signPos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Vector3 position2 = (signPos + IntVec3.North).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays); // shift up one cell
            Vector3 position = (position1 + position2) / 2; // midpoint*/

            Vector3 position = GenThing.TrueCenter(this.parent);
            position.y += (float)AltitudeLayer.MetaOverlays;
            Vector2 offset = GetLabelOffset();
            position.x += offset.x;
            position.z += offset.y;
            position.z += Settings.worldVerticalOffset; // TODO reevaluate this
            Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
            vector.y = (float)UI.screenHeight - vector.y;
            vector.y -= 1f;
            return vector;

            //return GenMapUI.LabelDrawPosFor(signPos + offset);
        }

        // Draws all the lines in signContents
        private void DrawSignLabels()
        {
            Color labelcolor = this.labelColor; // ignored for now. see declaration
            //labelcolor = Settings.globalLabelColor;

            Vector2 drawpos = GetSignLabelPos();

            foreach (string line in DoSignContents())
            {
                DrawSignLabel(drawpos, line, labelcolor);
                drawpos.y += Text.LineHeight * GetLineHeightBGFraction() * 1.1f;
                if (this.fontSize == GameFont.Medium)
                {
                    drawpos.y += 4f;
                }
            }
        }
        // Draws one line
        public void DrawSignLabel(Vector2 screenPos, string s, Color color)
        {
            if (s.Length <= 0)
            {
                return; // don't waste time drawing empty lines
            }

            s = Comp_Sign.UnescapeContents(s);

            // GenMapUI.DrawThingLabel(screenPos, s, color);
            // Content (mostly) copied from GenMapUI.DrawThingLabel()
            //Text.Font = GameFont.Medium;
            Text.Font = fontSize;
            Vector2 size = GetLabelSizeFor(s); //Text.CalcSize(s);
            float x = size.x;
            float y = size.y;

            Rect labelRect = new Rect(screenPos.x - size.x / 2f, screenPos.y - 3f, x, size.y);

            
            if (!labelRect.Overlaps(new Rect(0f, 0f, UI.screenWidth, UI.screenHeight)))
            {
                return; // cull offscreen labels
            }

            // Actually do the drawing
            GUI.DrawTexture(new Rect(screenPos.x - size.x / 2f - 4f, screenPos.y, size.x + 8f, size.y * GetLineHeightBGFraction()), TexUI.GrayTextBG);
            //GUI.DrawTexture(new Rect(screenPos.x - x / 2f - 4f, screenPos.y, x + 8f, y), TexUI.TextBGBlack);
            GUI.color = color;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, s);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // first return vanilla gizmos
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            // Don't even show the sign gizmos if the sign doesn't belong to the player faction
            if (this.parent.Faction == null || !this.parent.Faction.IsPlayer)
            {
                yield break;
            }

            // show/hide
            yield return new Command_Toggle
            {
                defaultLabel = "Signs_ShowGizmo".Translate(),
                defaultDesc = "Signs_ShowGizmo_desc".Translate(),
                hotKey = KeyBindingDefOf.Misc3,
                icon = TexCommand.ForbidOff,
                isActive = (() => !this.hideLabelOverride),
                toggleAction = delegate ()
                {
                    this.hideLabelOverride = !this.hideLabelOverride;
                }
            };
            // font size
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/SignSize", true),
                defaultLabel = GetSizeName(fontSize),
                defaultDesc = "Signs_SizeGismo_desc".Translate(),
                iconDrawScale = 1.2f,
                action = delegate ()
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    this.ChangeSize();
                }
            };

            // color
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Gizmo_Colorpicker", true),
                defaultLabel = "Signs_ColorGizmo".Translate(),
                defaultDesc = "Signs_SizeGismo_desc".Translate(),
                iconDrawScale = 1f,
                action = delegate ()
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    Find.WindowStack.Add(new Dialog_ColourPicker(this.labelColor,
                    (newColor) =>
                    {
                        this.labelColor = newColor;
                    }
                    ));
                }
            };


            if (canEditContent) // Don't add the edit gizmo or the copy/paste gizmos if the sign cannot be edited
            {
                // Rename/Edit gizmo
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true),
                    defaultLabel = "Signs_EditGizmo".Translate(),
                    defaultDesc = "Signs_EditGizmo_desc".Translate(),
                    action = delegate ()
                    {
                        if (EpitaphsLoaded && hasEpitaph && EpitaphComp != null)
                        {
                            
                            dynamic EpitaphComp_dynamic = EpitaphComp;
                            object[] ctorargs = { EpitaphComp_dynamic };
                            Window dlg = (Window)EpitaphAssem.CreateInstance("Epitaph.Dialog_EditEpitaph", false, BindingFlags.CreateInstance, null, ctorargs, null, null);
                            Find.WindowStack.Add(dlg);
                        }
                        else
                            Find.WindowStack.Add(new Dialog_RenameSign(this));
                    },
                    hotKey = KeyBindingDefOf.Misc1
                };

                if (hasEpitaph)
                {
                    // End early to cut off the remaining gizmos
                    yield break;
                }
                // copy/paste
                foreach (Gizmo gizmo in CommentContentClipboard.CopyPasteGizmosFor(this))
                {
                    yield return gizmo;
                }
            }
            yield break;
        }
        // Returns front-facing font names instead of the internal ones which are confusing
        private static string GetSizeName(GameFont f)
        {
            switch (f)
            {
                case GameFont.Tiny:
                    return "Signs_TinyFont".Translate();
                case GameFont.Small:
                    return "Signs_SmallFont".Translate();
                default:
                    return "Signs_MediumFont".Translate();
            }
        }
        public override string CompInspectStringExtra()
        {
            if (hasEpitaph)
            {
                // Don't modify the inspect string 
                return "";
            }

            StringBuilder stringBuilder = new StringBuilder();

            // Debug stuff
            /*stringBuilder.Append(hideLabelOverride ? "Hidden. " : "Visible. ");
            stringBuilder.Append("Size: ");
            stringBuilder.Append(Building_Sign.GetSizeName(fontSize));
            stringBuilder.AppendLine();*/

            if (this._signContent == null)
            {
                return "";
            }

            string trimmedContent = this.signContent;
            // Remove ending whitespace
            trimmedContent = trimmedContent.Trim();
            trimmedContent = Comp_Sign.UnescapeContents(trimmedContent);
            if (trimmedContent.Length <= 0)
            {
                // Don't add anything if there's no sign content
                return "";
            }
            trimmedContent = '"' + trimmedContent + '"';

            stringBuilder.Append("Signs_InspectStringPrefix".Translate());


            // Strip out empty lines
            foreach (string line in trimmedContent.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length > 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append(line);
                }
            }
            //trimmedContent = trimmedContent.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            //stringBuilder.Append(trimmedContent);

            return stringBuilder.ToString();
        }

        public static bool BuildableCanGoOverFog(BuildableDef def)
        {
            //TODO make this a DefModExtension
            if (def.defName == "Comment")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _signContent, "content");
            Scribe_Values.Look(ref _fontSize, "fontSize");
            Scribe_Values.Look(ref _labelColor, "labelColor", Settings.globalLabelColor);
            Scribe_Values.Look(ref hideLabelOverride, "hideLabelOverride");
        }
    }

    public class CompProperties_Sign : CompProperties
    {
        public bool canEditContent = true;
        public bool editOnPlacement = false;
        public bool isRoomSign = false;
        public bool canBeEmpty = false;
        public string defaultContents = "Empty sign";
        public Vector2 labelOffset = new Vector2(0, 0);
        public Vector2? labelOffset_East;
        public Vector2? labelOffset_West;
        public Vector2? labelOffset_South;
        public Vector2? labelOffset_North;

        public CompProperties_Sign()
        {
            this.compClass = typeof(Comp_Sign);
        }
        public CompProperties_Sign(Type compClass)
        {
            this.compClass = compClass;
        }
    }
}
