using HarmonyLib;
using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraFunctions
{
    [HarmonyPatch(typeof(DebugActionsIdeo))]
    [HarmonyPatch("SetIdeo")]
    public class DebugActionsIdeo_SetIdeo_ExtraFunctionsPatch
    {
        private static bool Prefix()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            IdeoFiles.RecacheData();
            List<Ideo> ideosListForReading = Find.IdeoManager.IdeosListForReading.Concat(IdeoFiles.AllIdeosLocal).ToList();
            for (int i = 0; i < ideosListForReading.Count; i++)
            {
                Ideo ideo = ideosListForReading[i];
                list.Add(new DebugMenuOption(ideo.name, DebugMenuOptionMode.Tool, delegate
                {
                    foreach (Pawn item in UI.MouseCell().GetThingList(Find.CurrentMap).OfType<Pawn>()
                        .ToList())
                    {
                        if (!item.RaceProps.Humanlike)
                        {
                            break;
                        }
                        if (Find.IdeoManager.IdeosListForReading.Contains(ideo) is false)
                        {
                            Find.IdeoManager.Add(ideo);
                        }
                        item.ideo.SetIdeo(ideo);
                        DebugActionsUtility.DustPuffFrom(item);
                    }
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            return false;
        }
    }
}
