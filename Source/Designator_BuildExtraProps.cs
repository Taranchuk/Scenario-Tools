using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace ExtraFunctions
{
    //public class Designator_BuildExtraProps : Designator_Build
    //{
    //    public Designator_BuildExtraProps(BuildableDef entDef) : base(entDef)
    //    {
    //        defaultDesc = "ExtraFunctions.Designator_BuildExtraProps.DefaultDesc".Translate(ExtraDesc);
    //    }
    //    private string ExtraDesc
    //    {
    //        get
    //        {
    //            StringBuilder stringBuilder = new StringBuilder();
    //            stringBuilder.Append("ExtraFunctions.SelectedFaction".Translate(selectedFaction == null ? "ExtraFunctions.None".Translate().ToString() : selectedFaction.GetCallLabel());
    //            return stringBuilder.ToString();
    //        }
    //    }
    //    //public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    //    //{
    //    //    get
    //    //    {
    //    //        foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
    //    //        {
    //    //            yield return new FloatMenuOption("ExtraFunctions.Designator_SetFaction.DefaultLabel".Translate($"{faction.GetCallLabel()} (defName = {faction.def.defName})"), delegate ()
    //    //            {
    //    //                selectedFaction = faction;
    //    //            });
    //    //        }
    //    //    }
    //    //}

    //}
	public class Designator_BuildExtraProps : Designator_Place
	{
		protected BuildableDef entDef;

		public Precept_Building sourcePrecept;

		private ThingDef stuffDef;

		private ThingDef resetStuffAfterDeselect;

		private bool writeStuff;
		public override BuildableDef PlacingDef => entDef;

		private Faction selectedFaction { get { return Utility.Designator_BuildExtraProps_SelectedFaction; } set { Utility.Designator_BuildExtraProps_SelectedFaction = value; } }
		private string ExtraDesc
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ExtraFunctions.SelectedFaction".Translate(selectedFaction == null ? "ExtraFunctions.None".Translate().ToString() : selectedFaction.GetCallLabel()));

				string result = stringBuilder.ToString();
				defaultDesc = "ExtraFunctions.Designator_BuildExtraProps.DefaultDesc".Translate(result);
				return result;
            }
        }
        public override ThingDef StuffDef
		{
			get
			{
				if (stuffDef != null)
				{
					return stuffDef;
				}
				ThingDef thingDef = null;
				if (entDef is ThingDef thingDef2 && thingDef2.MadeFromStuff)
				{
					thingDef = GenStuff.DefaultStuffFor(thingDef2);
					if (Find.CurrentMap != null && Find.CurrentMap.resourceCounter.GetCount(thingDef) < thingDef2.CostStuffCount)
					{
						ThingDef thingDef3 = null;
						foreach (KeyValuePair<ThingDef, int> allCountedAmount in Find.CurrentMap.resourceCounter.AllCountedAmounts)
						{
							if (allCountedAmount.Key.IsStuff && allCountedAmount.Key.stuffProps.canSuggestUseDefaultStuff && allCountedAmount.Key.stuffProps.CanMake(thingDef2) && allCountedAmount.Value >= thingDef2.CostStuffCount)
							{
								thingDef3 = allCountedAmount.Key;
								break;
							}
						}
						if (thingDef3 != null)
						{
							thingDef = thingDef3;
						}
					}
				}
				return thingDef;
			}
		}

		public ThingDef StuffDefRaw => stuffDef;

		public override string Label
		{
			get
			{
				ThingDef thingDef = entDef as ThingDef;
				string text = "";
				text = ((thingDef == null || !writeStuff) ? entDef.label : GenLabel.ThingLabel(thingDef, StuffDef));
				if (sourcePrecept != null)
				{
					text = sourcePrecept.TransformThingLabel(text);
				}
				if (thingDef != null && !writeStuff && thingDef.MadeFromStuff)
				{
					text += "...";
				}
				return text;
			}
		}

		public override string Desc => defaultDesc;

		public override Color IconDrawColor
		{
			get
			{
				if (entDef is ThingDef def)
				{
					Color? ideoColorForBuilding = IdeoUtility.GetIdeoColorForBuilding(def, Faction.OfPlayer);
					if (ideoColorForBuilding.HasValue)
					{
						return ideoColorForBuilding.Value;
					}
				}
				if (StuffDef != null)
				{
					return entDef.GetColorForStuff(StuffDef);
				}
				return entDef.uiIconColor;
			}
		}

		public override bool Visible => true;

		public override int DraggableDimensions => entDef.placingDraggableDimensions;

		public override bool DragDrawMeasurements => true;

		public override float PanelReadoutTitleExtraRightMargin => 20f;

		public override string HighlightTag
		{
			get
			{
				if (cachedHighlightTag == null && tutorTag != null)
				{
					cachedHighlightTag = "Designator-Build-" + tutorTag;
				}
				return cachedHighlightTag;
			}
		}

		public override ThingStyleDef ThingStyleDefForPreview
		{
			get
			{
				if (sourcePrecept == null)
				{
					return ThingStyleDefNonPreceptSource;
				}
				if (entDef is ThingDef thingDef)
				{
					return sourcePrecept.ideo.GetStyleFor(thingDef);
				}
				return null;
			}
		}

		public ThingStyleDef ThingStyleDefNonPreceptSource
		{
			get
			{
				if (PlacingDef is ThingDef thingDef)
				{
					return Faction.OfPlayer.ideos?.PrimaryIdeo?.GetStyleFor(thingDef);
				}
				return null;
			}
		}

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
                {
                    yield return new FloatMenuOption("ExtraFunctions.Designator_SetFaction.DefaultLabel".Translate($"{faction.GetCallLabel()} (defName = {faction.def.defName})"), delegate ()
                    {
                        selectedFaction = faction;
                    });
                }
            }
        }
        private void UpdateIcon()
		{
			if (entDef is TerrainDef)
			{
				iconTexCoords = Widgets.CroppedTerrainTextureRect(entDef.GetUIIconForStuff(StuffDef));
			}
		}

		public override void Deselected()
		{
			if (resetStuffAfterDeselect != null)
			{
				stuffDef = resetStuffAfterDeselect;
			}
			resetStuffAfterDeselect = null;
		}

		public Designator_BuildExtraProps(BuildableDef entDef)
		{
			this.entDef = entDef;
			iconAngle = entDef.uiIconAngle;
			iconOffset = entDef.uiIconOffset;
			hotKey = entDef.designationHotKey;
			tutorTag = entDef.defName;
			Order = 20f;
			if (entDef is ThingDef thingDef)
			{
				iconProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
				iconDrawScale = GenUI.IconDrawScale(thingDef);
			}
			else
			{
				iconProportions = new Vector2(1f, 1f);
				iconDrawScale = 1f;
			}
			defaultDesc = "ExtraFunctions.Designator_BuildExtraProps.DefaultDesc".Translate(ExtraDesc);
			ResetStuffToDefault();
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (entDef is ThingDef thingDef && thingDef.MadeFromStuff)
			{
				Designator_Dropdown.DrawExtraOptionsIcon(topLeft, GetWidth(maxWidth));
			}
			return result;
		}

		public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
		{
			ThingStyleDef thingStyleDef = ((PlacingDef is ThingDef) ? ThingStyleDefForPreview : null);
			Color? color = (parms.lowLight ? new Color?(Command.LowLightIconColor) : null);
			color = color ?? IconDrawColor;
			Widgets.DefIcon(rect, PlacingDef, StuffDef, 0.85f, thingStyleDef, drawPlaceholder: false, color);
		}

        public Texture ResolvedIcon(ThingStyleDef styleDef = null)
        {
            if (entDef is ThingDef thingDef)
            {
                ThingStyleDef thingStyleDef = styleDef ?? ThingStyleDefNonPreceptSource;
                return Widgets.GetIconFor(thingDef, StuffDef, thingStyleDef);
            }
            if (StuffDef != null && entDef.graphic is Graphic_Appearances graphic_Appearances)
            {
                return (Texture2D)graphic_Appearances.SubGraphicFor(StuffDef).MatAt(entDef.defaultPlacingRot).mainTexture;
            }
            return icon;
        }

        public void ResetStuffToDefault()
		{
			stuffDef = null;
			UpdateIcon();
		}

		public override void DrawPlaceMouseAttachments(float curX, ref float curY)
		{
			base.DrawPlaceMouseAttachments(curX, ref curY);
			if (ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				return;
			}
			DesignationDragger dragger = Find.DesignatorManager.Dragger;
			int num = ((!dragger.Dragging) ? 1 : dragger.DragCells.Count());
			List<ThingDefCountClass> list = entDef.CostListAdjusted(StuffDef);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				float y = curY;
				Widgets.ThingIcon(new Rect(curX, y, 27f, 27f), thingDefCountClass.thingDef);
				Rect rect = new Rect(curX + 29f, y, 999f, 29f);
				int num2 = num * thingDefCountClass.count;
				string text = num2.ToString();
				if (base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < num2)
				{
					GUI.color = Color.red;
					text += " (" + "NotEnoughStoredLower".Translate() + ")";
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect, text);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				curY += 29f;
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!CheckCanInteract())
			{
				return;
			}
			if (!(entDef is ThingDef thingDef) || !thingDef.MadeFromStuff)
			{
				base.ProcessInput(ev);
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingDef item in from d in base.Map.resourceCounter.AllCountedAmounts.Keys
									  orderby d.stuffProps?.commonality ?? float.PositiveInfinity descending, d.BaseMarketValue
									  select d)
			{
				if (item.IsStuff && item.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(item).Count > 0))
				{
					ThingDef localStuffDef = item;
					string text = "";
					text = ((sourcePrecept == null) ? GenLabel.ThingLabel(entDef, localStuffDef) : ((string)"ThingMadeOfStuffLabel".Translate(localStuffDef.LabelAsStuff, sourcePrecept.Label)));
					text = text.CapitalizeFirst();
					FloatMenuOption floatMenuOption = new FloatMenuOption(text, delegate
					{
						base.ProcessInput(ev);
						Find.DesignatorManager.Select(this);
						stuffDef = localStuffDef;
						writeStuff = true;
					}, item);
					floatMenuOption.tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName;
					list.Add(floatMenuOption);
				}
			}
			if (list.Count == 0)
			{
				Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			FloatMenu floatMenu = new FloatMenu(list);
			floatMenu.vanishIfMouseDistant = true;
			floatMenu.onCloseCallback = delegate
			{
				writeStuff = true;
			};
			Find.WindowStack.Add(floatMenu);
			Find.DesignatorManager.Select(this);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return GenConstruct.CanPlaceBlueprintAt(entDef, c, placingRot, base.Map, DebugSettings.godMode, null, null, StuffDef);
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
			{
				return;
			}
			if (entDef is TerrainDef)
			{
				base.Map.terrainGrid.SetTerrain(c, (TerrainDef)entDef);
			}
			else
			{
				Thing thing = ThingMaker.MakeThing((ThingDef)entDef, StuffDef);
				thing.SetFactionDirect(Faction.OfPlayer);
				Thing thing2 = GenSpawn.Spawn(thing, c, base.Map, placingRot);
				if (sourcePrecept != null && thing2 is Building building)
				{
					building.StyleSourcePrecept = sourcePrecept;
				}
				else if (sourcePrecept == null)
				{
					thing2.StyleDef = ThingStyleDefNonPreceptSource;
				}
			}
			FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size), base.Map);
			if (entDef is ThingDef thingDef2 && thingDef2.IsOrbitalTradeBeacon)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
			}
			if (TutorSystem.TutorialMode)
			{
				TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));
			}
			if (entDef.PlaceWorkers != null)
			{
				for (int i = 0; i < entDef.PlaceWorkers.Count; i++)
				{
					entDef.PlaceWorkers[i].PostPlace(base.Map, entDef, c, placingRot);
				}
			}
		}

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			BuildDesignatorUtility.TryDrawPowerGridAndAnticipatedConnection(entDef, placingRot);
		}

		public override void DrawPanelReadout(ref float curY, float width)
		{
			if (entDef.CostStuffCount <= 0 && stuffDef != null)
			{
				stuffDef = null;
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null)
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, thingDef, StuffDef);
			}
			else
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, entDef);
			}
			Text.Font = GameFont.Small;
			List<ThingDefCountClass> list = entDef.CostListAdjusted(StuffDef, errorOnNullStuff: false);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				Color color = GUI.color;
				Widgets.ThingIcon(new Rect(0f, curY, 20f, 20f), thingDefCountClass.thingDef);
				GUI.color = color;
				if (thingDefCountClass.thingDef != null && thingDefCountClass.thingDef.resourceReadoutPriority != 0 && base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < thingDefCountClass.count)
				{
					GUI.color = Color.red;
				}
				Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingDefCountClass.count.ToString());
				GUI.color = Color.white;
				string text = ((thingDefCountClass.thingDef != null) ? ((string)thingDefCountClass.thingDef.LabelCap) : ((string)("(" + "UnchosenStuff".Translate() + ")")));
				float width2 = width - 60f;
				float num = Text.CalcHeight(text, width2) - 5f;
				Widgets.Label(new Rect(60f, curY + 2f, width2, num + 5f), text);
				curY += num;
			}
			StyleCategoryDef styleCategoryDef = entDef.dominantStyleCategory;
			if (styleCategoryDef == null && thingDef != null)
			{
				styleCategoryDef = Faction.OfPlayer.ideos?.PrimaryIdeo?.GetStyleCategoryFor(thingDef);
			}
			if (styleCategoryDef != null)
			{
				TaggedString taggedString = "DominantStyle".Translate().CapitalizeFirst() + ": " + styleCategoryDef.LabelCap;
				Rect rect = new Rect(0f, curY + 2f, width, Text.CalcHeight(taggedString, width));
				Widgets.Label(rect, taggedString);
				curY += rect.height;
			}
			if (entDef.constructionSkillPrerequisite > 0)
			{
				DrawSkillRequirement(SkillDefOf.Construction, entDef.constructionSkillPrerequisite, width, ref curY);
			}
			if (entDef.artisticSkillPrerequisite > 0)
			{
				DrawSkillRequirement(SkillDefOf.Artistic, entDef.artisticSkillPrerequisite, width, ref curY);
			}
			bool flag = false;
			foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
			{
				if (freeColonist.skills.GetSkill(SkillDefOf.Construction).Level >= entDef.constructionSkillPrerequisite && freeColonist.skills.GetSkill(SkillDefOf.Artistic).Level >= entDef.artisticSkillPrerequisite)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				TaggedString taggedString2 = "NoColonistWithAllSkillsForConstructing".Translate(Faction.OfPlayer.def.pawnsPlural);
				Rect rect2 = new Rect(0f, curY + 2f, width, Text.CalcHeight(taggedString2, width));
				GUI.color = Color.red;
				Widgets.Label(rect2, taggedString2);
				GUI.color = Color.white;
				curY += rect2.height;
			}
			curY += 4f;
		}

		private bool AnyColonistWithSkill(int skill, SkillDef skillDef, bool careIfDisabled)
		{
			foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
			{
				if (freeColonist.skills.GetSkill(skillDef).Level >= skill && (!careIfDisabled || freeColonist.workSettings.WorkIsActive(WorkTypeDefOf.Construction)))
				{
					return true;
				}
			}
			return false;
		}

		private void DrawSkillRequirement(SkillDef skillDef, int requirement, float width, ref float curY)
		{
			Rect rect = new Rect(0f, curY + 2f, width, 24f);
			if (!AnyColonistWithSkill(requirement, skillDef, careIfDisabled: false))
			{
				GUI.color = Color.red;
				TooltipHandler.TipRegionByKey(rect, "NoColonistWithSkillTip", Faction.OfPlayer.def.pawnsPlural);
			}
			else if (!AnyColonistWithSkill(requirement, skillDef, careIfDisabled: true))
			{
				GUI.color = Color.yellow;
				TooltipHandler.TipRegionByKey(rect, "AllColonistsWithSkillHaveDisabledConstructingTip", Faction.OfPlayer.def.pawnsPlural, WorkTypeDefOf.Construction.gerundLabel);
			}
			else
			{
				GUI.color = new Color(0.72f, 0.87f, 0.72f);
			}
			Widgets.Label(rect, string.Format("{0}: {1}", "SkillNeededForConstructing".Translate(skillDef.LabelCap), requirement));
			GUI.color = Color.white;
			curY += 18f;
		}

		public void SetStuffDef(ThingDef stuffDef)
		{
			this.stuffDef = stuffDef;
			UpdateIcon();
		}

		public void SetStuffDefTemporary(ThingDef stuffDef)
		{
			resetStuffAfterDeselect = this.stuffDef;
			this.stuffDef = stuffDef;
			UpdateIcon();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
