using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraFunctions
{

    [HarmonyPatch(typeof(DebugToolsGeneral))]
    [HarmonyPatch("SetFaction")]
    public class DebugToolsGeneral_SetFaction_ExtraFunctionsPatch
    {
        private static bool Prefix()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Thing> things = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
            foreach (Faction item2 in Find.FactionManager.AllFactions)
            {
                Faction localFac = item2;
                FloatMenuOption item = new FloatMenuOption(localFac.Name, delegate
                {
                    foreach (Thing item3 in things)
                    {
                        if (item3.def.CanHaveFaction)
                        {
                            item3.SetFaction(localFac);
                        }
                    }
                });
                list.Add(item);
            }
            Find.WindowStack.Add(new FloatMenu(list));
            return false;
        }
    }


}
