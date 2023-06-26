using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraFunctions
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            var harmony = new Harmony("DimonSever000.ExtraFunctions");
            harmony.PatchAll();
        }
    }
}
