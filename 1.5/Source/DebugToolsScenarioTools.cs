using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using UnityEngine;
using System.Collections;
using Microsoft.SqlServer.Server;
using System.Security.Cryptography;
using static HarmonyLib.Code;
using LudeonTK;

namespace ExtraFunctions
{
    public static class DebugToolsScenarioTools
    {
        [DebugAction("Scenario Tools", "Clean area (rect)")]
        private static void ClearArea()
        {
            DebugToolsGeneral.GenericRectTool("Clean", delegate (CellRect rect)
            {
                foreach (IntVec3 cell in rect.Cells)
                {
                    FilthMaker.RemoveAllFilth(cell, Find.CurrentMap);
                }
            });
        }

        [DebugAction("Scenario Tools", "Generate plants (rect)")]
        private static void GeneratePlants()
        {
            List<ThingDef> plantDefs = new List<ThingDef>();
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("Single Plant", delegate
            {
                RecursiveSelect();

                void RecursiveSelect()
                {
                    List<FloatMenuOption> list1 = new List<FloatMenuOption>();
                    foreach (ThingDef plantDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.plant != null))
                    {
                        list1.Add(new FloatMenuOption(plantDef.LabelCap, delegate
                        {
                            plantDefs.Add(plantDef);
                            RecursiveSelect();
                        }));
                    }
                    list1.Add(new FloatMenuOption("end select", delegate
                    {
                        List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                        for (int i = 0; i < 11; i++)
                        {
                            int j = i;
                            list2.Add(new FloatMenuOption($"{j * 10}% ", delegate
                            {
                                if (plantDefs != null)
                                {
                                    DebugToolsGeneral.GenericRectTool("Generate plants", delegate (CellRect rect)
                                    {
                                        foreach (IntVec3 cell in rect.Cells)
                                        {
                                            if (Rand.Chance(j / 10f))
                                            {
                                                ThingDef plant = plantDefs.RandomElement();
                                                GenSpawn.Spawn(plant, cell, Find.CurrentMap);
                                            }
                                        }
                                    });
                                }

                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(list2));
                    }));

                    Find.WindowStack.Add(new FloatMenu(list1));
                }
            }));

            list.Add(new FloatMenuOption("Biome", delegate
            {
                List<FloatMenuOption> list1 = new List<FloatMenuOption>();
                foreach (BiomeDef biomeDef in DefDatabase<BiomeDef>.AllDefs)
                {
                    list1.Add(new FloatMenuOption(biomeDef.LabelCap, delegate
                    {
                        List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                        for (int i = 0; i < 11; i++)
                        {
                            int j = i;
                            list2.Add(new FloatMenuOption($"{j * 10}% ", delegate
                            {
                                if (plantDefs != null)
                                {
                                    DebugToolsGeneral.GenericRectTool("Generate plants", delegate (CellRect rect)
                                    {
                                        foreach (IntVec3 cell in rect.Cells)
                                        {
                                            if (Rand.Chance(j / 10f))
                                            {
                                                ThingDef plant = biomeDef.PlantCommonalities.Where(x => x.Key.plant?.pollution != Pollution.PollutedOnly).RandomElementByWeightWithDefault(x => x.Value, 0).Key;
                                                GenSpawn.Spawn(plant, cell, Find.CurrentMap);
                                            }
                                        }
                                    });
                                }

                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(list2));
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list1));
            }));

            Find.WindowStack.Add(new FloatMenu(list));
        }

        [DebugAction("Scenario Tools", "Set Material (rect)")]
        private static void SetMaterial()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (ThingDef stuff in DefDatabase<ThingDef>.AllDefs.Where(x => x.IsStuff))
            {
                list.Add(new FloatMenuOption(stuff.LabelCap, delegate
                {
                    DebugToolsGeneral.GenericRectTool("Set material", delegate (CellRect rect)
                    {
                        foreach (IntVec3 cell in rect.Cells)
                        {
                            foreach (Thing thing in cell.GetThingList(Find.CurrentMap))
                            {
                                if (thing.def.MadeFromStuff)
                                {
                                    thing.SetStuffDirect(stuff);
                                    thing.graphicInt = null;
                                    StatDefOf.MaxHitPoints.Worker.immutableStatCache?.Remove(thing);
                                    StatDefOf.MaxHitPoints.Worker.temporaryStatCache?.Remove(thing);
                                    if (thing.def.useHitPoints)
                                    {
                                        thing.HitPoints = thing.MaxHitPoints;
                                    }
                                    Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Buildings);
                                    Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Things);
                                    Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.BuildingsDamage);
                                }
                            }
                        }
                    });
                }));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        [DebugAction("Scenario Tools", "Grow plant to maturity (rect)")]
        private static void GrowPlantToMaturity()
        {
            DebugToolsGeneral.GenericRectTool("Grow plants to maturity", delegate (CellRect rect)
            {
                foreach (IntVec3 cell in rect.Cells)
                {
                    Plant plant = cell.GetPlant(Find.CurrentMap);
                    if (plant != null && plant.def.plant != null)
                    {
                        int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
                        plant.Age += num;
                        plant.Growth = 1f;
                        Find.CurrentMap.mapDrawer.SectionAt(cell).RegenerateAllLayers();
                    }
                }
            });
        }

        [DebugAction("Scenario Tools", "Make dormant (rect)")]
        private static void MakeDormant()
        {
            DebugToolsGeneral.GenericRectTool("Make dormant", delegate (CellRect rect)
            {
                foreach (IntVec3 cell in rect.Cells)
                {
                    List<Pawn> pawns = new List<Pawn>();
                    List<Thing> things = new List<Thing>();
                    foreach (Thing thing in cell.GetThingList(Find.CurrentMap))
                    {
                        CompCanBeDormant compCanBeDormant = thing.TryGetComp<CompCanBeDormant>();
                        if (thing is Pawn pawn && compCanBeDormant != null)
                        {
                            pawns.Add(pawn);
                            things.Add(pawn);
                            pawn.GetLord()?.RemovePawn(pawn);
                        }
                    }
                    if (!pawns.NullOrEmpty())
                    {
                        LordJob_SleepThenMechanoidsDefend lordJob_SleepThenMechanoidsDefend = new LordJob_SleepThenMechanoidsDefend(things, Faction.OfMechanoids, 5, cell, false, false);
                        LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob_SleepThenMechanoidsDefend, Find.CurrentMap, pawns);
                    }
                }
            });
        }

        [DebugAction("Scenario Tools", "Make Guard", actionType = DebugActionType.ToolMapForPawns)]
        private static void MakeGuard(Pawn p)
        {
            DebugToolsGeneral.GenericRectTool("Guard point", delegate (CellRect rect)
            {
                Thing defThing = null;
                foreach (IntVec3 cell in rect.Cells)
                {
                    defThing = cell.GetFirstThing<Thing>(Find.CurrentMap);
                    if (defThing != null)
                    {
                        break;
                    }
                }

                if (p != null && defThing != null)
                {
                    Lord lord = CompSpawnerPawn.FindLordToJoin(defThing, typeof(LordJob_MechanoidsDefend), true);
                    if (lord == null)
                    {
                        lord = CompSpawnerPawn.CreateNewLord(defThing, true, 5, typeof(LordJob_MechanoidsDefend));
                    }
                    p.GetLord()?.RemovePawn(p);
                    lord.AddPawn(p);
                }
            });
        }

        [DebugAction("Scenario Tools", "Make fog")]
        private static void MakeFog()
        {
            DebugToolsGeneral.GenericRectTool("Make fog", delegate (CellRect rect)
            {
                CellIndices cellIndices = Find.CurrentMap.cellIndices;
                foreach (IntVec3 cell in rect.Cells)
                {
                    if (!cell.Fogged(Find.CurrentMap))
                    {
                        Find.CurrentMap.fogGrid.fogGrid[cellIndices.CellToIndex(cell)] = true;
                        if (Current.ProgramState == ProgramState.Playing)
                        {
                            Find.CurrentMap.roofGrid.Drawer.SetDirty();
                            Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Things);
                            Find.CurrentMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.FogOfWar);
                        }
                    }
                }
            });

        }
        [DebugAction("Scenario Tools", "Remove fog")]
        private static void RemoveFog()
        {
            DebugToolsGeneral.GenericRectTool("Remove fog", delegate (CellRect rect)
            {
                CellIndices cellIndices = Find.CurrentMap.cellIndices;
                foreach (IntVec3 cell in rect.Cells)
                {
                    if (cell.Fogged(Find.CurrentMap))
                    {
                        Find.CurrentMap.fogGrid.Unfog(cell);
                    }
                }
            });

        }

        private static void MakeThing(Predicate<ThingDef> predicate)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (ThingDef apparel in DefDatabase<ThingDef>.AllDefs.Where(x => predicate(x)))
            {
                list.Add(new DebugMenuOption(apparel.defName, DebugMenuOptionMode.Action, delegate
                {
                    if (apparel.MadeFromStuff)
                    {
                        List<DebugMenuOption> list1 = new List<DebugMenuOption>();
                        foreach (ThingDef stuff in GenStuff.AllowedStuffsFor(apparel))
                        {
                            list1.Add(new DebugMenuOption(stuff.LabelCap, DebugMenuOptionMode.Action, delegate
                            {
                                if (apparel.HasComp(typeof(CompQuality)))
                                {
                                    List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                                    foreach (QualityCategory quality in Enum.GetValues(typeof(QualityCategory)).Cast<QualityCategory>())
                                    {
                                        list2.Add(new DebugMenuOption($"{quality}", DebugMenuOptionMode.Action, delegate
                                        {
                                            MakeThing(apparel, stuff, quality, true);
                                        }));
                                    }
                                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
                                }
                                else
                                {
                                    MakeThing(apparel, stuff, QualityCategory.Normal, false);
                                }
                            }));
                        }
                        Find.WindowStack.Add(new Dialog_DebugOptionListLister(list1));
                    }
                    else
                    {
                        if (apparel.HasComp(typeof(CompQuality)))
                        {
                            List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                            foreach (QualityCategory quality in Enum.GetValues(typeof(QualityCategory)).Cast<QualityCategory>())
                            {
                                list2.Add(new DebugMenuOption($"{quality}", DebugMenuOptionMode.Action, delegate
                                {
                                    MakeThing(apparel, null, quality, true);
                                }));
                            }
                            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
                        }
                        else
                        {
                            MakeThing(apparel, null, QualityCategory.Normal, false);
                        }
                    }
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));

            void MakeThing(ThingDef def, ThingDef stuff, QualityCategory qualityCategory, bool hasQuality)
            {
                DebugToolsGeneral.GenericRectTool("Make thing", delegate (CellRect rect)
                {
                    foreach (IntVec3 cell in rect.Cells)
                    {
                        Thing thing = ThingMaker.MakeThing(def, stuff);
                        if (hasQuality)
                        {
                            thing.TryGetComp<CompQuality>()?.SetQuality(qualityCategory, ArtGenerationContext.Outsider);
                        }
                        GenSpawn.Spawn(thing, cell, Find.CurrentMap);
                    }
                });
            }
        }

        [DebugAction("Scenario Tools", "Make Apparel")]
        private static void MakeApparel()
        {
            MakeThing(x => x.IsApparel);
        }

        [DebugAction("Scenario Tools", "Make Weapon")]
        private static void MakeWeapon()
        {
            MakeThing(x => x.IsWeapon);
        }

        [DebugAction("Scenario Tools", "Force sleep (rect) (enemy building)")]
        private static void ForceSleep()
        {
            DebugToolsGeneral.GenericRectTool("Force sleep (rect) (enemy building)", delegate (CellRect rect)
            {
                foreach (IntVec3 cell in rect.Cells)
                {
                    foreach (Thing thing in cell.GetThingList(Find.CurrentMap))
                    {
                        CompCanBeDormant compCanBeDormant = thing.TryGetComp<CompCanBeDormant>();
                        if (compCanBeDormant != null)
                        {
                            compCanBeDormant.ToSleep();
                        }
                    }
                }
            });
        }

        [DebugAction("Scenario Tools", "Init SpawnOnWakeup buildings (rect)")]
        private static void InitSpawnOnWakeupBuildings()
        {
            DebugToolsGeneral.GenericRectTool("Init buildings (rect)", delegate (CellRect rect)
            {
                foreach (IntVec3 cell in rect.Cells)
                {
                    foreach (Thing thing in cell.GetThingList(Find.CurrentMap))
                    {
                        CompPawnSpawnOnWakeup compPawnSpawnOnWakeup = thing.TryGetComp<CompPawnSpawnOnWakeup>();
                        if (compPawnSpawnOnWakeup != null)
                        {
                            compPawnSpawnOnWakeup.Initialize(compPawnSpawnOnWakeup.props);
                        }
                    }
                }
            });
        }

        [DebugAction("Scenario Tools", "Set faction (rect)", false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
        private static void SetFactionRect()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (Faction item2 in Find.FactionManager.AllFactions)
            {
                Faction localFac = item2;
                FloatMenuOption item = new FloatMenuOption(localFac.Name, delegate
                {
                    DebugToolsGeneral.GenericRectTool("Set faction", delegate (CellRect rect)
                    {
                        foreach (var cell in rect)
                        {
                            var things = Find.CurrentMap.thingGrid.ThingsAt(cell);
                            foreach (Thing item3 in things)
                            {
                                if (item3.def.CanHaveFaction)
                                {
                                    item3.SetFaction(localFac);
                                }
                            }
                        }
                    });
                });
                list.Add(item);
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        [DebugAction("Scenario Tools", "Remove research")]
        private static void RemoveResearch()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (var item2 in DefDatabase<ResearchProjectDef>.AllDefs.ToList())
            {
                var local = item2;
                FloatMenuOption item = new FloatMenuOption(local.LabelCap, delegate
                {
                    ResetResearch(local);
                });
                list.Add(item);
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private static void ResetResearch(ResearchProjectDef local)
        {
            Find.ResearchManager.progress[local] = 0;
            foreach (var research in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (research.PrerequisitesCompleted is false && research.IsFinished)
                {
                    ResetResearch(research);
                }
            }
        }
    }
}
