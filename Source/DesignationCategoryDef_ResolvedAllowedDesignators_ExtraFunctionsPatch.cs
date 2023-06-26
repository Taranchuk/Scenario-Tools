using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraFunctions
{
    [HarmonyPatch(typeof(DesignationCategoryDef))]
    [HarmonyPatch("ResolvedAllowedDesignators", MethodType.Getter)]
    public class DesignationCategoryDef_ResolvedAllowedDesignators_ExtraFunctionsPatch
    {
        private static void Postfix(ref IEnumerable<Designator> __result, ref DesignationCategoryDef __instance)
        {
            if (__instance != DesignationCategoryDefOfLocal.ExtraFunctions)
            {
                return;
            }
            List<Designator> list = __result.ToList();
            foreach (Designator item in GetThingsDesignators().ConcatIfNotNull(GetPawnDesignators()))
            {
                list.Add(item);
            }
            __result = list;
        }

        private static List<Designator> GetThingsDesignators()
        {
            List<Designator> list = new List<Designator>();
            foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (item.GetModExtension<DefModExtension_AddThingToPanel>() != null)
                {
                    list.Add(new Designator_BuildExtraProps(item));
                }
            }
            return list;
        }

        private static List<Designator> GetPawnDesignators()
        {
            List<Designator> list = new List<Designator>();
            foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (item.GetModExtension<DefModExtension_AddThingToPanel>() != null)
                {
                    list.Add(new Designator_SpawnPawn(item));
                }
            }
            return list;
        }
    }

}
