using System;
using System.Collections.Generic;
using System.Linq;
using Materia.Models;
using Verse;

namespace Materia
{
    internal class RecipesGen
    {
        public static readonly string[] BasicCrops =
        {
            "RawRice", "RawPotatoes", "RawCorn", "RawBerries"
        };

        private readonly Random _random;
        private const double MeatChance = 0.5;
        private readonly HashSet<ThingDef> _plantIngredients;
        private readonly HashSet<ThingDef> _meatIngredients;
        private readonly HashSet<ThingDef> _animalProductIngredients;

        public RecipesGen(Random random)
        {
            _random = random;

            var baseRecipe = DefDatabase<RecipeDef>.GetNamed("MateriaBaseCookingRecipe");

            var ingredients = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => baseRecipe.defaultIngredientFilter.Allows(t))
                .ToHashSet();

            _plantIngredients = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "PlantFoodRaw"))
                .ToHashSet();

            _meatIngredients = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "MeatRaw"))
                .ToHashSet();

            _animalProductIngredients = ingredients
                .Where(t => t.thingCategories.Any(d => d.defName == "AnimalProductRaw"))
                .ToHashSet();
        }

        public void Populate(MateriaDatabase database, IEnumerable<RecipeDef> recipeDefs)
        {
            database.RecipeSpecs.Clear();

            const int amountPerTier = 9;
            int i = 0;

            foreach (var def in recipeDefs)
            {
                var spec = new RecipeSpec
                {
                    Name = def.defName,
                    Label = $"Materia Recipe No. {++i}",
                    Description = $"A materia recipe with the number {i}.",
                    ProductLabel = $"Meal No. {i}",
                    ProductDescription = $"A meal with the number {i}.",
                    IsUnlocked = _random.Next(0, 2) == 0
                };

                if (i < amountPerTier) { SetStatsT1(spec); }
                else if (i < 2 * amountPerTier) { SetStatsT2(spec); }
                else if (i < 3 * amountPerTier) { SetStatsT3(spec); }
                else if (i < 4 * amountPerTier) { SetStatsT4(spec); }
                else { SetStatsT5(spec); }

                database.RecipeSpecs.Add(spec);
            }
        }

        private void SetStatsT1(RecipeSpec spec)
        {
            spec.DaysToRot = _random.Next(1, 6);
            spec.MarketValue = _random.Next(5, 30);
            spec.Mass = NextFloat(0.30, 0.60);
            spec.Nutrition = NextFloat(0.5, 0.6);
            spec.Yield = _random.Next(1, 3);
            spec.WorkToMake = _random.Next(300, 400) * spec.Yield;
            spec.Skill = _random.Next(0, 4);

            int min = 7 * spec.Yield;
            int max = 15 * spec.Yield;

            spec.Ingredients.Add(GetBasicCropOrAnyMeat(min, max));
        }

        private void SetStatsT2(RecipeSpec spec)
        {
            spec.DaysToRot = _random.Next(3, 9);
            spec.MarketValue = _random.Next(20, 50);
            spec.Mass = NextFloat(0.30, 0.60);
            spec.Nutrition = NextFloat(0.6, 0.7);
            spec.Yield = _random.Next(1, 4);
            spec.WorkToMake = _random.Next(400, 500) * spec.Yield;
            spec.Skill = _random.Next(4, 8);

            int min = 7 * spec.Yield;
            int max = 15 * spec.Yield;

            var used = new HashSet<string>();
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
        }

        private void SetStatsT3(RecipeSpec spec)
        {
            spec.DaysToRot = _random.Next(6, 12);
            spec.MarketValue = _random.Next(40, 60);
            spec.Mass = NextFloat(0.30, 0.60);
            spec.Nutrition = NextFloat(0.7, 0.8);
            spec.Yield = _random.Next(2, 5);
            spec.WorkToMake = _random.Next(500, 600) * spec.Yield;
            spec.Skill = _random.Next(8, 11);

            int min = 7 * spec.Yield;
            int max = 15 * spec.Yield;

            var used = new HashSet<string>();
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
        }

        private void SetStatsT4(RecipeSpec spec)
        {
            spec.DaysToRot = _random.Next(6, 12);
            spec.MarketValue = _random.Next(50, 70);
            spec.Mass = NextFloat(0.30, 0.60);
            spec.Nutrition = NextFloat(0.8, 0.9);
            spec.Yield = _random.Next(2, 5);
            spec.WorkToMake = _random.Next(650, 800) * spec.Yield;
            spec.Skill = _random.Next(11, 15);

            int min = 7 * spec.Yield;
            int max = 15 * spec.Yield;

            var used = new HashSet<string>();
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
        }

        private void SetStatsT5(RecipeSpec spec)
        {
            spec.DaysToRot = _random.Next(6, 12);
            spec.MarketValue = _random.Next(60, 80);
            spec.Mass = NextFloat(0.30, 0.60);
            spec.Nutrition = 1.0f;
            spec.Yield = _random.Next(2, 5);
            spec.WorkToMake = _random.Next(800, 900) * spec.Yield;
            spec.Skill = _random.Next(15, 19);

            int min = 7 * spec.Yield;
            int max = 15 * spec.Yield;

            var used = new HashSet<string>();
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
            spec.Ingredients.Add(GePlantOrAnyMeat(used, min, max));
        }

        private float NextFloat(double minimum, double maximum)
        {
            return (float)(_random.NextDouble() * (maximum - minimum) + minimum);
        }

        private IngredientSpec GetBasicCropOrAnyMeat(int amountMin, int amountMax)
        {
            int amount = _random.Next(amountMin, amountMax);

            if (_random.NextDouble() < MeatChance) { return new IngredientSpec {AnyMeat = true, Amount = amount}; }

            string ing = BasicCrops
                .OrderBy(c => Guid.NewGuid())
                .First();

            return new IngredientSpec {Name = ing, Amount = amount};
        }

        private IngredientSpec GePlantOrAnyMeat(ICollection<string> used, int amountMin, int amountMax)
        {
            int amount = _random.Next(amountMin, amountMax);

            if (!used.Contains("Meat") && _random.NextDouble() < MeatChance)
            {
                used.Add("Meat");
                return new IngredientSpec {Name = "Meat", AnyMeat = true, Amount = amount};
            }

            string name = _plantIngredients
                .Where(t => !used.Contains(t.defName))
                .OrderBy(c => Guid.NewGuid())
                .First().defName;

            used.Add(name);

            return new IngredientSpec {Name = name, Amount = amount};
        }
    }
}
