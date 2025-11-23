using HarmonyLib;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace RimSweeper
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            try
            {
                var harmony = new Harmony("krabgor.rimsweeper");
                harmony.PatchAll();
            }
            catch (System.Exception e)
            {
                Log.Error("[RimSweeper] Failed to initialize Harmony patches: " + e.ToString());
            }
        }
    }

    [HarmonyPatch]
    public static class PathFinder_FindPath_Patch
    {
        static MethodBase TargetMethod()
        {
            // Robust search: Find any method in PathFinder that returns PawnPath
            var methods = AccessTools.GetDeclaredMethods(typeof(PathFinder));
            foreach (var m in methods)
            {
                if (m.ReturnType == typeof(PawnPath))
                {
                    // If there are multiple, usually the one with the most parameters is the main one
                    // But let's just return the first one we find for now, or filter by param count if needed.
                    // In 1.5 there is only one public FindPath, but maybe internal helpers.
                    if (m.Name.Contains("FindPath"))
                    {
                        return m;
                    }
                }
            }
            
            // If we are here, we failed.
            Log.Error("[RimSweeper] Could not find PathFinder.FindPath method. Smart avoidance will not work.");
            return null;
        }
        public static int GetExtraCost(int baseCost, Map map, int cellIndex, Pawn pawn)
        {
            // If pawn is colonist and cell is marked, add cost
            if (pawn != null && pawn.IsColonist)
            {
                var comp = RS_AvoidGridComponent.GetComponent(map);
                if (comp != null && comp.ShouldAvoid(cellIndex))
                {
                    return baseCost + 1000;
                }
            }
            return baseCost;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            FieldInfo pathGridArrField = AccessTools.Field(typeof(PathGrid), "pathGrid");
            MethodInfo getExtraCostMethod = AccessTools.Method(typeof(PathFinder_FindPath_Patch), nameof(GetExtraCost));

            // We need to track if pathGrid is stored in a local variable
            int pathGridLocalIndex = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                // 1. Detect if pathGrid is stored in a local
                // Ldfld PathGrid.pathGrid
                // Stloc.s V_X
                if (codes[i].LoadsField(pathGridArrField))
                {
                    if (i + 1 < codes.Count && codes[i+1].IsStloc())
                    {
                        // Found it being stored in a local
                        if (codes[i+1].opcode == OpCodes.Stloc_0) pathGridLocalIndex = 0;
                        else if (codes[i+1].opcode == OpCodes.Stloc_1) pathGridLocalIndex = 1;
                        else if (codes[i+1].opcode == OpCodes.Stloc_2) pathGridLocalIndex = 2;
                        else if (codes[i+1].opcode == OpCodes.Stloc_3) pathGridLocalIndex = 3;
                        else if (codes[i+1].opcode == OpCodes.Stloc_S) pathGridLocalIndex = ((LocalBuilder)codes[i+1].operand).LocalIndex;
                        else if (codes[i+1].opcode == OpCodes.Stloc) pathGridLocalIndex = ((LocalBuilder)codes[i+1].operand).LocalIndex;
                    }
                }

                yield return codes[i];

                // 2. Check for array access
                if (codes[i].opcode == OpCodes.Ldelem_I4)
                {
                    bool isPathGridAccess = false;

                    // Case A: Direct field access (Ldfld pathGrid -> ... -> Ldelem)
                    // Check previous instructions
                    if (i > 1 && codes[i-2].LoadsField(pathGridArrField))
                    {
                        isPathGridAccess = true;
                    }
                    // Case B: Local variable access (Ldloc pathGrid -> ... -> Ldelem)
                    else if (pathGridLocalIndex != -1 && i > 1)
                    {
                        // Check if [i-2] loads the local we found earlier
                        var prev = codes[i-2];
                        if ((prev.opcode == OpCodes.Ldloc_0 && pathGridLocalIndex == 0) ||
                            (prev.opcode == OpCodes.Ldloc_1 && pathGridLocalIndex == 1) ||
                            (prev.opcode == OpCodes.Ldloc_2 && pathGridLocalIndex == 2) ||
                            (prev.opcode == OpCodes.Ldloc_3 && pathGridLocalIndex == 3) ||
                            ((prev.opcode == OpCodes.Ldloc_S || prev.opcode == OpCodes.Ldloc) && 
                             prev.operand is LocalBuilder lb && lb.LocalIndex == pathGridLocalIndex))
                        {
                            isPathGridAccess = true;
                        }
                    }

                    if (isPathGridAccess)
                    {
                        // Inject call to GetExtraCost
                        // Stack: [cost]
                        
                        // Load Map
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PathFinder), "map"));
                        
                        // Load Index (clone from i-1)
                        yield return codes[i-1].Clone(); 
                        
                        // Load Pawn
                        yield return new CodeInstruction(OpCodes.Ldarg_3); 
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(TraverseParms), "Pawn"));
                        
                        // Call
                        yield return new CodeInstruction(OpCodes.Call, getExtraCostMethod);
                    }
                }
            }
        }
    }
}
