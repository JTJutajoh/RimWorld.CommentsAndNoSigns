using System;
//using System.Web;
//using System.Security;
using RimWorld;
using Verse;
using UnityEngine;
using ColourPicker;

namespace Dark.Signs
{
    //ripped from Dialog_RenameZone and LWM Deep Storage's version of that
    public class Dialog_RenameSign : Dialog_Rename
    {
        private Color curColor;

        //private Building_Sign sign;
        private Comp_Sign signComp;
        protected override int MaxNameLength //unused
        {
            get
            {
                return Settings.characterLimit;
            }
        }

        private Vector2 scrollbarPos = new Vector2(0f, 0f);

        /*public Dialog_RenameSign(Building_Sign sign)
        {
            this.sign = sign;
            this.curName = sign.signContent;
        }*/
        public Dialog_RenameSign(Comp_Sign signComp)
        {
            this.signComp = signComp;
            this.curName = signComp?.signContent ?? "";
            this.curColor = signComp?.labelColor ?? Color.white.ToOpaque();
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport result = new AcceptanceReport();
            result = true;
            //AcceptanceReport result = base.NameIsValid(name); // makes sure it's longer than 0
            if (!this.signComp.Props.canBeEmpty)
            {
                result = base.NameIsValid(name); // only checks 0 length
            }
            if (Settings.useCharacterLimit)
            {
                result = CheckLength(name);
            }

            if (!result.Accepted)
            {
                return result;
            }
            return true;
        }

        private AcceptanceReport CheckLength(string name)
        {
            if (name.Length > Settings.characterLimit)
            {
                AcceptanceReport result = new AcceptanceReport("Signs_TooLong".Translate());
                return result;
            }
            return true;
        }

        // Sanitize the user's input so that no one comes to me complaining about corrupted saves
        // because they thought it would be a bright idea to put xml tags in their sign
        private string SanitizeInput(string raw)
        {
            string sanitized = raw;
            sanitized = System.Security.SecurityElement.Escape(raw);

            return sanitized;
        }

        protected override void SetName(string name)
        {
            /*if (this.sign != null)
            {
                this.sign.signContent = name.Trim();
            }*/

            name = SanitizeInput(name);

            if (this.signComp != null)
            {
                this.signComp.signContent = name.Trim();
                SetColor(this.curColor);
            }
            else
            {
                Log.Error("(Signs) Edit dialog has no sign or signComp to edit.");
            }
            Messages.Message("Signs_SetSign".Translate(), MessageTypeDefOf.TaskCompletion, false);
        }

        protected void SetColor(Color color)
        {
            if (this.signComp != null)
            {
                this.signComp.labelColor = this?.curColor ?? Color.white;
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                var o = base.InitialSize;
                o.x += 200f;
                o.y += 100f;
                
                return o;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            bool enterPressed = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                //enterPressed = true; // Shift was not held
                this.scrollbarPos.y += 20f;
                Event.current.Use();
            }
            GUI.SetNextControlName("RenameField");
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "Signs_EditHeader".Translate());// (max " + this.MaxNameLength.ToString() + " chars):");
            Widgets.Label(new Rect(0f, 30f, inRect.width, 30f), "Signs_LengthWarning".Translate());
            if (Settings.useCharacterLimit)
            {
                Rect LenghlimitLabel = new Rect(inRect.width - (inRect.width / 2.5f), 144f, inRect.width / 2.5f, 24f);
                Widgets.Label(LenghlimitLabel, "Signs_LengthLeft".Translate() + " " + (this.MaxNameLength - this.curName.Length));
                if (this.curName.Length > this.MaxNameLength)
                {
                    Widgets.DrawBoxSolid(LenghlimitLabel, Color.red.ToTransparent(0.6f));
                }
            }

            // Body
            Rect bodyRect = new Rect(inRect.width * 0.13f, 64f, inRect.width*0.9f, 70f);
            string text = Widgets.TextAreaScrollable(bodyRect, Comp_Sign.UnescapeContents(this.curName), ref this.scrollbarPos);
            this.curName = text;
                

            // Clear button
            Rect ClearButRect = new Rect(0, 65f, (inRect.width * 0.12f), 64f);
            if (Widgets.ButtonText(ClearButRect, "Signs_ClearButton".Translate(), true, true, Color.red, true))
            {
                this.curName = "";
            }
            Widgets.DrawBoxSolid(ClearButRect, new Color(0.2f, 0f, 0f, 0.6f));

            // Color picker button
            Rect ColorButRect = new Rect(0f, 144f, inRect.width / 3f, 32f);
            Rect ColorSwatchRect = new Rect(inRect.width / 3f + 8f, 144f, 32f, 32f);
            Widgets.DrawBoxSolid(ColorSwatchRect, this.curColor);
            Vector2 ColorPickerPos = new Vector2(UI.screenWidth - InitialSize.x, UI.screenHeight - InitialSize.y) / 2f;
            ColorPickerPos.y += inRect.height * 1.15f;
            if (Widgets.ButtonText(ColorButRect, "Signs_ColorButton".Translate(), true, true, true) || Widgets.ButtonInvisible(ColorSwatchRect, true))
            {
                Find.WindowStack.Add(new Dialog_ColourPicker(this.curColor,
                (newColor) =>
                {
                    this.curColor = newColor;
                    SetColor(newColor);
                },
                position: ColorPickerPos
                ) );
            }
                //Widgets.DrawBoxSolid(ColorButRect, this.curColor.ToTransparent(0.3f));

            //int charsLeft = this.MaxNameLength - 2 - text.Length;
            //Widgets.Label(new Rect(0f, 112f, inRect.width, 30f), charsLeft.ToString() + " characters remaining");

            if (Widgets.ButtonText(new Rect(0f, inRect.height - 45f, inRect.width, 36f), "OK", true, true, true) || enterPressed)
            {
                AcceptanceReport acceptanceReport = this.NameIsValid(this.curName);
                if (!acceptanceReport.Accepted)
                {
                    if (acceptanceReport.Reason.NullOrEmpty())
                    {
                        Messages.Message("Signs_BlankMessage".Translate(), MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                    Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                else
                {
                    this.SetName(this.curName);
                    Find.WindowStack.TryRemove(this, true);
                }
            }
        }
    }
}
