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
	// TODO make a clipboard class/struct instead of a bunch of fields
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
		// Used by Comp_Sign to add copy/paste gizmos for sign settings
		public static IEnumerable<Gizmo> CopyPasteGizmosFor(Comp_Sign sign)
		{
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
				defaultLabel = "Signs_CopyGizmo".Translate(),
				defaultDesc = "Signs_CopyGizmo_desc".Translate(),
				action = delegate ()
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					CommentContentClipboard.CopyFrom(ref sign);
				},
				hotKey = KeyBindingDefOf.Misc4
			};
			Command_Action command_Action = new Command_Action();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true);
			command_Action.defaultLabel = "Signs_PasteGizmo".Translate();
			command_Action.defaultDesc = "Signs_PasteGizmo_desc".Translate();
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
