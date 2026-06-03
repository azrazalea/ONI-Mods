using FUtility;
using HarmonyLib;
using PrintingPodRecharge.Content.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrintingPodRecharge.Patches
{
    public class EntityConfigManagerPatch
    {
        [HarmonyPatch(typeof(EntityConfigManager), "LoadGeneratedEntities")]
        public class EntityConfigManager_LoadGeneratedEntities_Patch
        {
            // Raw (un-cooked) food costs this multiple of the prepared cost, so prepared food is the
            // better deal but raw still works (keeps the ink craftable on lean asteroids).
            private const float RAW_FOOD_MULTIPLIER = 3f;

            private static HashSet<Tag> preparedTags;
            private static Dictionary<string, Func<Tag[]>> uniformCategories;

            public static void Postfix()
            {
                for (var i = 0; i < Mod.Recipes.BioInks.Count; i++)
                {
                    var recipe = Mod.Recipes.BioInks[i];

                    if (recipe.Outputs[0].ID == BioInkConfig.MEDICINAL && !Mod.otherMods.IsDiseasesExpandedHere)
                        continue;

                    // Build inputs, expanding category / food markers. A null means an ingredient is
                    // unavailable (empty category, or a specific item whose prefab doesn't exist), so
                    // skip the recipe.
                    var inputs = recipe.Inputs.Select(BuildIngredient).ToArray();
                    if (inputs.Any(e => e == null))
                    {
                        Log.Debug($"Skipping bio-ink recipe '{recipe.Outputs[0].ID}': an ingredient is unavailable.");
                        continue;
                    }

                    var outputs = recipe.Outputs
                        .Select(output => new ComplexRecipe.RecipeElement(output.ID, output.Amount))
                        .ToArray();

                    CreateRecipe(CraftingTableConfig.ID, inputs, outputs, recipe.Description, recipe.Time, i);
                }
            }

            // "Any member of this category" ingredients that take a single uniform amount.
            // (Food is handled separately because it needs per-food amounts.)
            private static Dictionary<string, Func<Tag[]>> UniformCategories
            {
                get
                {
                    if (uniformCategories == null)
                    {
                        uniformCategories = new Dictionary<string, Func<Tag[]>>
                        {
                            // Any refined metal (Copper, Iron, Gold, Aluminium, Cobalt, Tungsten, ...).
                            { GameTags.RefinedMetal.ToString(), () => ElementTags(GameTags.RefinedMetal) },
                            // Any crop seed.
                            { GameTags.CropSeed.ToString(),     () => PrefabTags(GameTags.CropSeed) },
                            // Any critter egg.
                            { GameTags.Egg.ToString(),          () => PrefabTags(GameTags.Egg) },
                        };
                    }
                    return uniformCategories;
                }
            }

            private static ComplexRecipe.RecipeElement BuildIngredient(Settings.Recipes.FRecipeElement input)
            {
                // Quality-segregated food, with raw (non-cooked) variants weighted heavier.
                if (input.ID == BioInkConfig.FOOD_LOW_TAG)
                    return BuildFood(input.Amount, q => q <= TUNING.FOOD.FOOD_QUALITY_GOOD);
                if (input.ID == BioInkConfig.FOOD_HIGH_TAG)
                    return BuildFood(input.Amount, q => q >= TUNING.FOOD.FOOD_QUALITY_GREAT);

                // Other "any member of this category" ingredients (uniform amount).
                if (UniformCategories.TryGetValue(input.ID, out var gather))
                {
                    var members = gather();
                    if (members == null || members.Length == 0)
                    {
                        Log.Debug($"Bio-ink category '{input.ID}' has no available members; recipe skipped.");
                        return null;
                    }
                    return new ComplexRecipe.RecipeElement(members, input.Amount);
                }

                // A specific material; must have a prefab to be craftable on this save.
                Tag tag = input.ID;
                if (Assets.TryGetPrefab(tag) == null)
                {
                    Log.Debug("Bio-ink ingredient is not available: " + input.ID);
                    return null;
                }
                return new ComplexRecipe.RecipeElement(tag, input.Amount);
            }

            // Foods matching the quality predicate, each weighted: prepared (a cooking-recipe result)
            // costs baseAmount; raw food costs RAW_FOOD_MULTIPLIER x baseAmount. Returns a single
            // ingredient whose possibleMaterials are the foods with per-material amounts, which the
            // game expands into one craftable variant per food under a single material picker.
            private static ComplexRecipe.RecipeElement BuildFood(float baseAmount, Func<int, bool> qualityOk)
            {
                var prepared = PreparedTags();
                var tags = new List<Tag>();
                var amounts = new List<float>();

                foreach (var go in Assets.GetPrefabsWithComponent<Edible>())
                {
                    var edible = go.GetComponent<Edible>();
                    if (edible?.FoodInfo == null || !qualityOk(edible.FoodInfo.Quality))
                        continue;

                    var prefabId = go.GetComponent<KPrefabID>();
                    if (prefabId == null || Assets.TryGetPrefab(prefabId.PrefabTag) == null)
                        continue;

                    var isPrepared = prepared.Contains(prefabId.PrefabTag);
                    tags.Add(prefabId.PrefabTag);
                    amounts.Add(baseAmount * (isPrepared ? 1f : RAW_FOOD_MULTIPLIER));
                }

                if (tags.Count == 0)
                {
                    Log.Debug("No foods matched a bio-ink food recipe; recipe skipped.");
                    return null;
                }
                return new ComplexRecipe.RecipeElement(tags.ToArray(), amounts.ToArray());
            }

            // Tags that are the RESULT of some ComplexRecipe = "prepared" / manufactured. For food this
            // distinguishes cooked dishes (Mush Bar, Liceloaf, BBQ, ...) from raw produce / meat / eggs
            // (which come from plants and critters, not recipes). Cooking recipes are registered by
            // building configs during LoadGeneratedEntities, before this postfix, so the set is complete.
            private static HashSet<Tag> PreparedTags()
            {
                if (preparedTags == null)
                {
                    preparedTags = new HashSet<Tag>();
                    var mgr = ComplexRecipeManager.Get();
                    foreach (var r in mgr.preProcessRecipes)
                        foreach (var res in r.results)
                            preparedTags.Add(res.material);
                    foreach (var r in mgr.recipes)
                        foreach (var res in r.results)
                            preparedTags.Add(res.material);
                }
                return preparedTags;
            }

            // All refined-metal ELEMENTS with a craftable prefab. Metals are elements, so they come
            // from ElementLoader rather than the loose-prefab registry.
            private static Tag[] ElementTags(Tag categoryTag)
            {
                return ElementLoader.elements
                    .Where(e => e.HasTag(categoryTag) && Assets.TryGetPrefab(e.tag) != null)
                    .Select(e => e.tag)
                    .Distinct()
                    .ToArray();
            }

            // All loose prefabs carrying a tag (crop seeds, critter eggs, ...).
            private static Tag[] PrefabTags(Tag tag)
            {
                return Assets.GetPrefabsWithTag(tag)
                    .Select(go => go.GetComponent<KPrefabID>())
                    .Where(p => p != null)
                    .Select(p => p.PrefabTag)
                    .Distinct()
                    .ToArray();
            }

            public static void CreateRecipe(string fabricatorID, ComplexRecipe.RecipeElement[] input, ComplexRecipe.RecipeElement[] output, string description, float time, int sortOrderOffset)
            {
                var recipeID = ComplexRecipeManager.MakeRecipeID(fabricatorID, input, output);

                var desc = Strings.TryGet(description, out var result) ? result.String : description;

                new ComplexRecipe(recipeID, input, output)
                {
                    time = time,
                    description = desc,
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                    fabricators = new List<Tag>
                    {
                        TagManager.Create(fabricatorID)
                    },
                    sortOrder = sortOrderOffset + 30
                };
            }
        }
    }
}
