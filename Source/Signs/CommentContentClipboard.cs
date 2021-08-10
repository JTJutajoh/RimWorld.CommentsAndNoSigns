 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Dark.Signs
{
    public static class CommentContentClipboard
    {
        private static string copiedString = "";
		private static bool copiedHide = false;
		private static GameFont copiedFont = GameFont.Tiny;
		private static Color copiedColor = Color.white;
        private static bool copied = false;
        public static bool HasCopiedContent
        {
            get 
            {
                return CommentContentClipboard.copied;
            }
        }

        public static void CopyFrom(ref Comp_Sign sign)
        {
			string c = sign.signContent;
            CommentContentClipboard.copiedString = string.Copy(c);
            CommentContentClipboard.copiedHide = sign.hideLabelOverride;
            CommentContentClipboard.copiedFont = sign.fontSize;
			CommentContentClipboard.copiedColor = sign.labelColor;
            CommentContentClipboard.copied = true;
        }
        public static void PasteInto(ref Comp_Sign sign)
        {
            sign.signContent = CommentContentClipboard.copiedString;
			sign.fontSize = CommentContentClipboard.copiedFont;
			sign.hideLabelOverride = CommentContentClipboard.copiedHide;
			sign.labelColor = CommentContentClipboard.copiedColor;

		}
		public static IEnumerable<Gizmo> CopyPasteGizmosFor(Comp_Sign sign)
		{
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
				defaultLabel = "Copy Sign",
				defaultDesc = "Copy this sign's settings so you can paste them onto another sign",
				action = delegate ()
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					CommentContentClipboard.CopyFrom(ref sign);
				},
				hotKey = KeyBindingDefOf.Misc4
			};
			Command_Action command_Action = new Command_Action();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true);
			command_Action.defaultLabel = "Paste Sign";
			command_Action.defaultDesc = "Overwrite this sign's content and settings with those previously copied to the clipboard";
			command_Action.action = delegate ()
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				CommentContentClipboard.PasteInto(ref sign);
			};
			command_Action.hotKey = KeyBindingDefOf.Misc5;
			if (!CommentContentClipboard.HasCopiedContent)
			{
				command_Action.Disable(null);
			}
			yield return command_Action;
			yield break;
		}
	}
}
