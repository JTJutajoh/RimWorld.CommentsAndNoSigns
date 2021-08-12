using System;
//using System.Web;
//using System.Security;
using RimWorld;
using Verse;
using UnityEngine;

namespace Dark.Signs
{
    //ripped from Dialog_RenameZone and LWM Deep Storage's version of that
    public class Dialog_RenameSign : Dialog_Rename
    {
        //private Building_Sign sign;
        private Comp_Sign signComp;
        protected override int MaxNameLength //unused
        {
            get
            {
                return 128;
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
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            //AcceptanceReport result = base.NameIsValid(name); // makes sure it's longer than 0
            AcceptanceReport result = new AcceptanceReport();
            if (!this.signComp.Props.canBeEmpty)
            {
                result = base.NameIsValid(name);
            }
            else
            {
                result = true;
            }
            if (!result.Accepted)
            {
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
            }
            else
            {
                Log.Error("(Signs) Edit dialog has no sign or signComp to edit.");
            }
            Messages.Message("Sign content set", MessageTypeDefOf.TaskCompletion, false);
        }

        public override Vector2 InitialSize
        {
            get
            {
                var o = base.InitialSize;
                o.x += 165f;
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
            Widgets.Label(new Rect(0f, 4f, inRect.width, 30f), "Enter the text for this sign");// (max " + this.MaxNameLength.ToString() + " chars):");
            string text = Widgets.TextAreaScrollable(new Rect(0f, 32f, inRect.width, 70f), Comp_Sign.UnescapeContents(this.curName), ref this.scrollbarPos);
            this.curName = text;

            if (Widgets.ButtonText(new Rect(inRect.width/4, 114f, inRect.width/2, 35f), "Clear", true, true, true))
            {
                this.curName = "";
            }

            //int charsLeft = this.MaxNameLength - 2 - text.Length;
            //Widgets.Label(new Rect(0f, 112f, inRect.width, 30f), charsLeft.ToString() + " characters remaining");
            Widgets.Label(new Rect(0f, 160f, inRect.width, 30f), "Beware that many lines of text could impact performance.");
                
            if (Widgets.ButtonText(new Rect(15f, inRect.height - 45f, inRect.width - 30f, 35f), "OK", true, true, true) || enterPressed)
            {
                AcceptanceReport acceptanceReport = this.NameIsValid(this.curName);
                if (!acceptanceReport.Accepted)
                {
                    if (acceptanceReport.Reason.NullOrEmpty())
                    {
                        Messages.Message("Cannot be blank", MessageTypeDefOf.RejectInput, false);
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
