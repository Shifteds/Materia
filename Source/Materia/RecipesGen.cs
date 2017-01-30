using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Materia.Components;
using Materia.Defs;
using Materia.Models;
using RimWorld;
using Verse;
using S = Materia.Settings;

namespace Materia
{
    internal class RecipesGen
    {
        private readonly Random _random;
        private readonly List<IngredientOption> _plantsAndAnyMeat;
        private readonly List<IngredientOption> _meats;
        private readonly List<IngredientOption> _basicCropsAndAnyMeat;
        private readonly List<IngredientOption> _animalProducts;
        private readonly List<FlavorText> _flavor;
        private readonly List<HediffSpec> _hediffOptions;

        private readonly HashSet<string> _combinations = new HashSet<string>();
        private readonly HashSet<string> _ingredients = new HashSet<string>();

        public RecipesGen(Random random)
        {
            _random = random;

            var ti = new CultureInfo("en-US", false).TextInfo;

            var baseRecipe = DefDatabase<RecipeDef>.GetNamed("MateriaBaseCookingRecipe");

            var ingredients = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => baseRecipe.defaultIngredientFilter.Allows(t))
                .ToHashSet();

            _plantsAndAnyMeat = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "PlantFoodRaw"))
                .Select(t => new IngredientOption { Name = t.defName, Label = ti.ToTitleCase(t.label) })
                .ToList();

            _plantsAndAnyMeat.Add(new IngredientOption { Name = "Meat", Label = "Meat" });

            _meats = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "MeatRaw"))
                .Select(t => new IngredientOption { Name = t.defName, Label = ti.ToTitleCase(t.label) })
                .ToList();

            _animalProducts = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "AnimalProductRaw") || t.ingestible?.foodType == FoodTypeFlags.AnimalProduct)
                .Select(t => new IngredientOption { Name = t.defName, Label = ti.ToTitleCase(t.label) })
                .ToList();

            _basicCropsAndAnyMeat = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => t.plant?.sowTags != null && t.plant.sowTags.Contains("Ground") && t.plant.harvestedThingDef != null)
                .Select(t => t.plant.harvestedThingDef)
                .Where(t => ingredients.Contains(t))
                .Select(t => new IngredientOption { Name = t.defName, Label = ti.ToTitleCase(t.label) })
                .ToList();

            _basicCropsAndAnyMeat.Add(new IngredientOption { Name = "Meat", Label = "Meat" });

            _flavor = DefDatabase<FlavorTextDef>.AllDefsListForReading
                .SelectMany(d => d.Entries)
                .ToList();

            _hediffOptions = DefDatabase<HediffDef>.AllDefsListForReading
                .Where(h => h.HasComp(typeof(RecipeGenStatsComp)))
                .Select(GetSpec)
                .OrderBy(g => Guid.NewGuid())
                .ToList();
        }

        public void Populate(MateriaDatabase database, Queue<RecipeDef> recipeDefs)
        {
            database.RecipeSpecs.Clear();

            for (int i = 0; i < 3; i++)
            {
                var options = GetOptions(i).ToList();
                var recipes = GenerateTier(i, 3, options, recipeDefs);
                database.RecipeSpecs.AddRange(recipes);

                Logger.Log($"Generated {recipes.Count} T{i + 1} recipes.");
            }

            var flavors = GetFlavorMap();

            int it = 0;
            var usedHediffs = new HashSet<string>();
            foreach (var recipe in database.RecipeSpecs)
            {
                SetFlavor(it++, flavors, recipe);
                SetHediffs(recipe, usedHediffs);
            }
        }

        private float NextFloat(double minimum, double maximum)
        {
            return (float)(_random.NextDouble() * (maximum - minimum) + minimum);
        }

        private List<RecipeSpec> GenerateTier(int tier, int rerollAttempts, List<IngredientOption> options, Queue<RecipeDef> recipeDefs)
        {
            int recipeAmount = S.RecipeAmount[tier];
            int ingredientCount = S.IngredientCount[tier];
            int minIngAmount = S.MinIngredientAmounts[tier];
            int maxIngAmount = S.MaxIngredientAmounts[tier];

            var recipes = new List<RecipeSpec>(recipeAmount);

            for (int i = 0; i < recipeAmount; i++)
            {
                if(recipeDefs.Count == 0) { break; }

                var recipe = new RecipeSpec();
                recipe.Name = recipeDefs.Dequeue().defName;
                recipe.Tier = tier + 1;
                recipe.DaysToRot = _random.Next(S.DaysToRotMin[tier], S.DaysToRotMax[tier]);
                recipe.MarketValue = _random.Next(S.MarketValueMin[tier], S.MarketValueMax[tier]);
                recipe.Mass = NextFloat(0.05, 0.60);
                recipe.Nutrition = NextFloat(0.4, 0.9);
                recipe.Yield = _random.Next(S.YieldMin[tier], S.YieldMax[tier]);
                recipe.WorkToMake = _random.Next(S.WorkToMakeMin[tier], S.WorkToMakeMax[tier]) * recipe.Yield;
                recipe.Skill = _random.Next(S.CookingSkillMin[tier], S.CookingSkillMax[tier]);
                recipe.ProgressGain = (int)(S.ProgressGainPerTier[tier]/recipe.Yield);

                var ingredients = Enumerable.Range(0, rerollAttempts)
                    .Select(s => Roll(ingredientCount, minIngAmount, maxIngAmount, recipe.Yield, options))
                    .Select(s => new {Guid = s.Select(g => g.Name).Aggregate((a, b) => a + b), Ing = s})
                    .OrderByDescending(c => !_combinations.Contains(c.Guid))
                    .ThenBy(c => c.Ing.Count(ing => _ingredients.Contains(ing.Name)))
                    .First();

                _combinations.Add(ingredients.Guid);
                foreach (var ing in ingredients.Ing) { _ingredients.Add(ing.Name); }
                recipe.Ingredients = ingredients.Ing;

                recipes.Add(recipe);
            }

            return recipes;
        }

        private List<IngredientSpec> Roll(int count, int minAmount, int maxAmount, int yield, IEnumerable<IngredientOption> options)
        {
            var uniqueOptions = options.ToList();
            int num = Math.Min(count, uniqueOptions.Count);
            var ingredients = new List<IngredientSpec>();

            for (int i = 0; i < num; i++)
            {
                var option = uniqueOptions[_random.Next(0, uniqueOptions.Count)];

                var ing = new IngredientSpec
                {
                    Name = option.Name,
                    Label = option.Label,
                    Amount = (_random.Next(minAmount, maxAmount) * yield)/count
                };

                ingredients.Add(ing);
                uniqueOptions.Remove(option);
            }

            return ingredients.OrderBy(i => i.Name).ToList();
        }

        private static void SetFlavor(int iteration, IDictionary<string, List<FlavorText>> map, RecipeSpec spec)
        {
            FlavorText flavor = null;
            var ingredientNames = spec.Ingredients.Select(i => i.Name).ToHashSet();

            foreach (var ing in spec.Ingredients)
            {
                List<FlavorText> list;
                if (!map.TryGetValue(ing.Name.ToLower(), out list)) { continue; }

                flavor = list.FirstOrDefault(f => f.Ingredients.All(i => ingredientNames.Contains(i)));
                if (flavor == null) { continue; }

                list.Remove(flavor);
                break;
            }

            if (flavor != null)
            {
                spec.Label = $"Make {flavor.Name}";
                spec.ProductLabel = flavor.Name;
                spec.Description = flavor.Description;
                spec.ProductDescription = flavor.Description;

                return;
            }

            spec.Label = $"Make {spec.Ingredients.First().Label} Meal No. {iteration}";
            spec.ProductLabel = $"{spec.Ingredients.First().Label} Meal No. {iteration}";

            var description = new StringBuilder("A meal made of");
            for (int i = 0; i < spec.Ingredients.Count - 1; i++) { description.Append($" {spec.Ingredients[i].Label},"); }

            if (spec.Ingredients.Count == 2) { description = description.Replace(",", ""); }

            description.Append(spec.Ingredients.Count > 1
                ? $" and {spec.Ingredients[spec.Ingredients.Count - 1].Label}."
                : $" {spec.Ingredients[spec.Ingredients.Count - 1].Label}.");

            spec.Description = description.ToString();
            spec.ProductDescription = spec.Description;
        }

        private IEnumerable<IngredientOption> GetOptions(int tier)
        {
            switch (tier)
            {
                case 0:
                    return _basicCropsAndAnyMeat;
                case 1:
                    return _plantsAndAnyMeat
                        .Union(_animalProducts);
                case 2:
                    return _plantsAndAnyMeat
                        .Union(_meats)
                        .Union(_animalProducts);
                default:
                    return new List<IngredientOption>();
            }
        }

        private Dictionary<string, List<FlavorText>> GetFlavorMap()
        {
            return _flavor
                .SelectMany(f => f.Ingredients)
                .GroupBy(i => i)
                .Select(g => g.Key)
                .ToDictionary(i => i.ToLower(), i => _flavor.Where(a => a.Ingredients.Contains(i)).OrderBy(a => Guid.NewGuid()).ToList());
        }

        private HediffSpec GetSpec(HediffDef def)
        {
            var stats = def.CompProps<RecipeGenStats>();
            float deviation = stats.Deviation;
            float value = stats.Value + NextFloat(-deviation, deviation);

            return new HediffSpec
            {
                Tier = stats.Tier,
                Name = def.defName,
                Label = def.label,
                Value = value,
                IsPositive = stats.IsPositive,
                StatType = def.stages.First().statOffsets.First().stat.defName
            };
        }

        private void SetHediffs(RecipeSpec spec, HashSet<string> used)
        {
            int tier = spec.Tier;
            var usedTypes = new HashSet<string>();

            for (int i = 0; i < tier; i++)
            {
                var next = _hediffOptions
                    .Where(h => h.Tier == tier && h.IsPositive)
                    .OrderByDescending(h => !used.Contains(h.Name))
                    .ThenByDescending(h => !usedTypes.Contains(h.StatType))
                    .FirstOrDefault();

                if (next == null) { break; }

                spec.Hediffs.Add(next);
                used.Add(next.Name);
                usedTypes.Add(next.StatType);
            }

            var negative = _hediffOptions
                .Where(h => h.Tier == tier && !h.IsPositive)
                .OrderByDescending(h => !used.Contains(h.Name))
                .ThenByDescending(h => !usedTypes.Contains(h.StatType))
                .FirstOrDefault();

            if (negative == null) { return; }

            used.Add(negative.Name);
            spec.Hediffs.Add(negative);
        }
    }
}
