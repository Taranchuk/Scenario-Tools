using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace ExtraFunctions
{
    public class Designator_SpawnPawn : Designator
    {
        public override int DraggableDimensions => 2;
        private Faction selectedFaction { get { return Utility.Designator_SpawnPawn_SelectedFaction; } set { Utility.Designator_SpawnPawn_SelectedFaction = value; } }
        private bool sleep { get { return Utility.Designator_SpawnPawn_Sleep; } set { Utility.Designator_SpawnPawn_Sleep = value; } }

        private PawnKindDef pawnKindDef;
        CompProperties_CanBeDormant compProperties_CanBeDormant => pawnKindDef.race.GetCompProperties<CompProperties_CanBeDormant>();
        public Designator_SpawnPawn(PawnKindDef pawnKindDef)
        {
            this.pawnKindDef = pawnKindDef;
            defaultLabel = pawnKindDef.LabelCap;
            defaultDesc = "ExtraFunctions.Designator_SpawnPawn.DefaultDesc".Translate(ExtraDesc);
            icon = pawnKindDef.race.uiIcon;
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Cancel;
            hotKey = KeyBindingDefOf.Designator_Cancel;
        }

        private string ExtraDesc
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("ExtraFunctions.SelectedFaction".Translate(selectedFaction == null ? "ExtraFunctions.None".Translate().ToString() : selectedFaction.GetCallLabel()));
                stringBuilder.Append("ExtraFunctions.SleepNow".Translate(sleep ? "ExtraFunctions.Yes".Translate() : "ExtraFunctions.No".Translate()));
                return stringBuilder.ToString();
            }
        }
        protected virtual void UpdateDesc()
        {
            defaultDesc = "ExtraFunctions.Designator_SpawnPawn.DefaultDesc".Translate(ExtraDesc);
            Deselected();
        }
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                yield return new FloatMenuOption("ExtraFunctions.SetFaction".Translate(), delegate ()
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
                    {
                        list.Add(new FloatMenuOption($"{faction.GetCallLabel()} (defName = {faction.def.defName})", delegate ()
                        {
                            selectedFaction = faction;
                            UpdateDesc();
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                });
                if (compProperties_CanBeDormant != null)
                {
                    yield return new FloatMenuOption("ExtraFunctions.SetSleep".Translate(), delegate ()
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        list.Add(new FloatMenuOption("ExtraFunctions.Yes".Translate(), delegate ()
                        {
                            sleep = true;
                            UpdateDesc();
                        }));
                        list.Add(new FloatMenuOption("ExtraFunctions.No".Translate(), delegate ()
                        {
                            sleep = false;
                            UpdateDesc();
                        }));
                        Find.WindowStack.Add(new FloatMenu(list));
                    });
                }
            }
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            return c.Standable(Map);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, selectedFaction);
            GenSpawn.Spawn(pawn, c, Map);
            Lord lord = null;
            if (sleep)
            {
                lord = ((Pawn)GenClosest.ClosestThing_Global(c, Map.mapPawns.SpawnedPawnsInFaction(selectedFaction), 99999f, (Thing p) => p != pawn && ((Pawn)p).GetLord() is Lord l 
                && l.LordJob.GetType() == typeof(LordJob_SleepThenMechanoidsDefend))).GetLord();
                if (lord == null)
                {
                    lord = LordMaker.MakeNewLord(selectedFaction, new LordJob_SleepThenMechanoidsDefend(new List<Thing> { pawn }, selectedFaction, 30, c, canAssaultColony: false, isMechCluster: false), 
                        Map, new List<Pawn> { pawn });
                }
                else
                {
                    lord.AddPawn(pawn);
                }
                ForceSleep(pawn);
            }
            else
            {
                lord = ((Pawn)GenClosest.ClosestThing_Global(c, Map.mapPawns.SpawnedPawnsInFaction(selectedFaction), 99999f, (Thing p) => p != pawn && ((Pawn)p).GetLord() is Lord l 
                && l.LordJob.GetType() == typeof(LordJob_DefendPoint))).GetLord();
                if (lord == null)
                {
                    lord = LordMaker.MakeNewLord(selectedFaction, new LordJob_DefendPoint(c, 20),
                        Map, new List<Pawn> { pawn });
                }
                else
                {
                    lord.AddPawn(pawn);
                }
            }
        }

        private static void ForceSleep(Pawn pawn)
        {
            CompCanBeDormant compCanBeDormant = pawn.TryGetComp<CompCanBeDormant>();
            if (compCanBeDormant != null)
            {
                compCanBeDormant.ToSleep();
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
