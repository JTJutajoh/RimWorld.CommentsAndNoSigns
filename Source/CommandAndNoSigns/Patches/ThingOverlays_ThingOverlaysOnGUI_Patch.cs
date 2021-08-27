using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit; // for OpCodes
using Verse;
using RimWorld;
using HarmonyLib;

namespace Dark.Signs
{
    
    [HarmonyPatch(typeof(ThingOverlays))]
    [HarmonyPatch("ThingOverlaysOnGUI")]

    // Patch to ignore fog for Things with defName "Comment"
    // to allow Comment signs to draw their label on top of fog if they are placed in fog
    //
    //TODO change from checking for a hard-coded defName to checking for a defModExtension
    public static class ThingOverlays_ThingOverlaysOnGUI_Patch
    {
        //// Original code:
        // if (currentViewRect.Contains(thing.Position) && !Find.CurrentMap.fogGrid.IsFogged(thing.Position))
        //// Transpiled code:
        // if (currentViewRect.Contains(thing.Position) && (thing.def.defName == "Comment" || !Find.CurrentMap.fogGrid.IsFogged(thing.Position)))
        //
        // Need to inject 'thing.def.defName == "Comment"' expression and skip over IsFogged() check if true
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
        {
            bool foundAnchorOpCode = false;
            var nopLabel = generator.DefineLabel();

            // copy the original method's instructions into a list that we can modify and return
            var codes = new List<CodeInstruction>(instructions);

            //// Strategy:
            // Look for the brfalse.s instruction from the first condition of the if condition
            // Then, once we've found its, inject another set of instructions to check another condition after it
            // this injected condition short-circuits the if statement by jumping over the orignal second condition's instructions if true

            
            // IL we are looking to match with comment breakdown:

            // load local variable 'thing' onto stack
            //IL_0039: ldloc.3
            // get location of 'thing' and check if it is within camera
			//IL_003a: callvirt instance valuetype Verse.IntVec3 Verse.Thing::get_Position()
			//IL_003f: call instance bool Verse.CellRect::Contains(valuetype Verse.IntVec3)
            // if not, fail conditional and jump past the code within the if statement
			//IL_0044: brfalse.s IL_0093 // <== IL we are looking to match
                
            ////INJECTED IL
            // load local variable 'thing' onto stack
			//ldloc.3
            // Call GetShouldDrawOverFog passing 'thing' and push result to stack
            //call bool Dark.Signs::ShouldDrawOverFog(valuetype Verse.Thing)
            //brfalse.s IL_0093 // result was false, do not draw over fog, skip over the body of the if (which draws)
            ////END INJECTED IL
             

            for (var i = 0; i < codes.Count; i++)
            {
                // Return unchanged instructions
                if (!(codes[i].opcode == OpCodes.Nop && foundAnchorOpCode)) // Exclude the nop code we want to branch to, as we're going to emit a modified version
                {
                    yield return codes[i]; // return this instruction unchanged
                }

                if (codes[i].opcode == OpCodes.Brfalse_S) // check if this was our anchor instruction
                {
                    foundAnchorOpCode = true;

                    // output our injected instructions
                    yield return new CodeInstruction(OpCodes.Ldloc, 3);
                    yield return CodeInstruction.Call(typeof(ThingOverlays_ThingOverlaysOnGUI_Patch), "ShouldDrawOverFog");
                    yield return new CodeInstruction(OpCodes.Brtrue_S, nopLabel);

                    //lastIndex = i + 3; // expand the number of instructions by the 3 we injected

                    //break;
                    //startIndex = i + 1; // save the location of the instruction after our anchor
                }
                if (codes[i].opcode == OpCodes.Nop && foundAnchorOpCode)
                {
                    CodeInstruction nopCode = codes[i];
                    nopCode.labels.Add(nopLabel);
                    yield return nopCode;
                }
            }

            //return codes.AsEnumerable();
            //yield break;
        }

        public static bool ShouldDrawOverFog(Thing thing)
        {
            return Comp_Sign.BuildableCanGoOverFog(thing.def);
        }
    }
    
}
