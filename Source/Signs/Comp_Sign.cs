using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Dark.Signs
{
    public class Comp_Sign : ThingComp
    {
        public CompProperties_Sign Props => (CompProperties_Sign)this.props;

        public bool canEditContent => Props.canEditContent;
        public bool editOnPlacement => Props.editOnPlacement;
        public bool isRoomSign => Props.isRoomSign;


        public bool hideLabelOverride = false;

        // init to null so we can check on spawn if there's any existing content
        private string _signContent;
        public string signContent
        {
            get
            {
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

        // This property is ignored for now, in favor of getting the global setting at draw time
        private Color _labelColor = Settings.globalLabelColor;
        public Color labelColor
        {
            get
            {
                return _labelColor;
            }
            set
            {
                _labelColor = value;
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
            }
        }

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
            if (ShowCommentToggle.drawComments == false)
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
                taggedString = room.Role.PostProcessedLabelCap;
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

        private IEnumerable<string> DoSignContents()
        {
            if (this._signContent == null)
            {
                yield break;
            }
            else
            {
                foreach (string s in this._signContent.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.None))
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
        private float GetLineHeight()
        {
            switch (this.fontSize)
            {
                case GameFont.Tiny:
                    return 0.6f;
                case GameFont.Small:
                    return 0.7f;
                case GameFont.Medium:
                    return 0.9f;
                default:
                    return 0.6f;
            }
        }
        private Vector2 GetLabelBGSizeFor(string s)
        {
            Vector2 size = new Vector2();
            //size = Text.CalcSize(s);
            GUIContent content = new GUIContent(s);
            size = Text.CurFontStyle.CalcSize(content);
            size.y *= GetLineHeight();
            return size;
        }

        // returns the direction-specific offset for the current rotation if defined. If not, returns the generic labelOffset
        private Vector2 GetLabelOffset()
        {
            Rot4 rot = this.parent.Rotation;
            Vector2 result;
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
            labelcolor = Settings.globalLabelColor;

            Vector2 drawpos = GetSignLabelPos();

            foreach (string line in DoSignContents())
            {
                DrawSignLabel(drawpos, line, labelcolor);
                drawpos.y += Text.LineHeight * GetLineHeight() * 1.1f;
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
            Vector2 size = GetLabelBGSizeFor(s); //Text.CalcSize(s);
            float x = size.x;
            float y = size.y;
            GUI.DrawTexture(new Rect(screenPos.x - x / 2f - 4f, screenPos.y, x + 8f, y), TexUI.GrayTextBG);
            GUI.color = color;
            Text.Anchor = TextAnchor.UpperCenter;
            //Widgets.Label(new Rect(screenPos.x - x / 2f, screenPos.y - 3f, x, 999f), s);
            Widgets.Label(new Rect(screenPos.x - x / 2f, screenPos.y - 3f, x, 999f), s);
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
            if (this.parent.Faction == null)
            {
                yield break;
            }

            if (!this.parent.Faction.IsPlayer)
            {
                yield break;
            }

            yield return new Command_Toggle
            {
                defaultLabel = "Show Label",
                defaultDesc = "When active, show the label all the time. When inactive, the label will only be shown if this sign is selected. Use if you have many signs that you don't need to see all the time.",
                hotKey = KeyBindingDefOf.Misc3,
                icon = TexCommand.ForbidOff,
                isActive = (() => !this.hideLabelOverride),
                toggleAction = delegate ()
                {
                    this.hideLabelOverride = !this.hideLabelOverride;
                }
            };

            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/SignSize", true),
                defaultLabel = GetSizeName(fontSize),
                defaultDesc = "Change the size of the label for this sign. There are 3 sizes available.",
                iconDrawScale = 1.2f,
                action = delegate ()
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    this.ChangeSize();
                }
            };

            if (canEditContent) // Don't add the edit gizmo or the copy/paste gizmos if the sign cannot be edited
            {
                // Rename/Edit gizmo
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true),
                    defaultLabel = "Edit",
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new Dialog_RenameSign(this));
                    },
                    hotKey = KeyBindingDefOf.Misc1
                };

                foreach (Gizmo gizmo in CommentContentClipboard.CopyPasteGizmosFor(this))
                {
                    yield return gizmo;
                }
            }
            yield break;
        }
        private static string GetSizeName(GameFont f)
        {
            switch (f)
            {
                case GameFont.Tiny:
                    return "Small";
                case GameFont.Small:
                    return "Medium";
                default:
                    return "Large";
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            /*stringBuilder.Append(hideLabelOverride ? "Hidden. " : "Visible. ");
            stringBuilder.Append("Size: ");
            stringBuilder.Append(Building_Sign.GetSizeName(fontSize));
            stringBuilder.AppendLine();*/

            if (this._signContent == null)
            {
                return "";
            }

            string trimmedContent = Comp_Sign.UnescapeContents(this._signContent.Trim());
            if (trimmedContent.Length <= 0)
            {
                // Don't add anything if there's no sign content
                return "";
            }
            trimmedContent = '"' + trimmedContent + '"';

            stringBuilder.Append("Sign Contents:");


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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _signContent, "content");
            Scribe_Values.Look(ref _fontSize, "fontSize");
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
