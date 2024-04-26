using HarmonyLib;
using LudeonTK;
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
    [HarmonyPatch(typeof(DebugToolsSpawning))]
    [HarmonyPatch("CreateMealWithSpecifics")]
    public class DebugToolsSpawning_CreateMealWithSpecifics_ExtraFunctionsPatch
    {
        private static float poisonChance = 0f;
        private static bool Prefix()
        {
            poisonChance = 0f;
            IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsNutritionGivingIngestible && x.ingestible.IsMeal);
            IEnumerable<ThingDef> ingredientDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsNutritionGivingIngestible && x.ingestible.HumanEdible && !x.ingestible.IsMeal && !x.IsCorpse);
            ThingDef mealDef = null;
            List<ThingDef> ingredients = new List<ThingDef>();
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (ThingDef d in enumerable)
            {
                list.Add(new DebugMenuOption(d.defName, DebugMenuOptionMode.Action, delegate
                {
                    mealDef = d;

                    poisonChance = 0f;

                    List<DebugMenuOption> list1 = new List<DebugMenuOption>();
                    for (int i = 0; i < 11; i++)
                    {
                        int j = i;
                        list1.Add(new DebugMenuOption($"{j * 10}% ", DebugMenuOptionMode.Action, delegate
                        {
                            poisonChance = j / 10f;
                            Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetIngredientOptions()));
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list1));
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));

            IEnumerable<DebugMenuOption> GetIngredientOptions()
            {
                yield return new DebugMenuOption("[Finish" + ingredients.Count + " / " + 3 + "]", DebugMenuOptionMode.Tool, delegate
                {
                    Thing thing = ThingMaker.MakeThing(mealDef);
                    CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        compIngredients.RegisterIngredient(ingredients[i]);
                    }
                    if (Rand.Chance(poisonChance))
                    {
                        CompFoodPoisonable foodPoisonable = thing.TryGetComp<CompFoodPoisonable>();
                        if (foodPoisonable != null)
                        {
                            typeof(CompFoodPoisonable).GetField("poisonPct", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static).SetValue(foodPoisonable, poisonChance);
                        }
                    }
                    GenPlace.TryPlaceThing(thing, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);

                });
                yield return new DebugMenuOption("[Clear selections]", DebugMenuOptionMode.Action, delegate
                {
                    ingredients.Clear();
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetIngredientOptions()));
                });
                foreach (ThingDef item in ingredientDefs)
                {
                    ThingDef ingredient = item;
                    if (ingredients.Count < 3 && FoodUtility.MealCanBeMadeFrom(mealDef, ingredient))
                    {
                        string label = (ingredients.Contains(ingredient) ? (ingredient.defName + " ✓") : ingredient.defName);
                        yield return new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
                        {
                            if (ingredients.Contains(ingredient))
                            {
                                ingredients.Remove(ingredient);
                            }
                            else
                            {
                                ingredients.Add(ingredient);
                            }
                            Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetIngredientOptions()));
                        });
                    }
                }
            }
            return false;
        }
    }
}
