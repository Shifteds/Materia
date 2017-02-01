using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Utils;
using Materia.Gen;
using Materia.Models;
using RimWorld;
using Verse;

namespace Materia
{
    public class MateriaMod : ModBase
    {
        private static readonly Random _random = new Random();
        private MateriaDatabase _database;
        private readonly List<RecipeSpec> _emptySpecs = new List<RecipeSpec>();
        private RecipeSpec _currentCache;

        public bool RemoveCookingRecipes = true;

        public static MateriaMod Instance { get; private set; }

        public MateriaMod()
        {
            Instance = this;
        }

        public override string ModIdentifier => "Materia";

        public override void DefsLoaded()
        {
            if (!ModIsActive) { return; }

            RemoveCookingRecipes = Settings.GetHandle("removeCookingRecipes", "Remove Normal Recipes".Translate(), 
                "Removes every cooking recipe that produces a meal, except for Simple Meal and Pemmican.".Translate(), true);

            if (RemoveCookingRecipes) { RemoveNormalCookingRecipes(); }
            EffectsGen.CreateEmptyHediffs();
        }

        public override void WorldLoaded()
        {
            if (!ModIsActive) { return; }
            _currentCache = null;

            var recipes = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.defName.StartsWith("MateriaRecipe"))
                .ToDictionary(r => r.defName, r => r);

            _database = UtilityWorldObjectManager.GetUtilityWorldObject<MateriaDatabase>();

            if (_database.RecipeSpecs.Count == 0)
            {
                Logger.Message($"Generating {recipes.Count} new recipe.");

                var recipeGen = new RecipesGen(_random);
                recipeGen.Populate(_database, recipes.Values.ToQueue());
                EffectsGen.PopulateEffects(_database.RecipeSpecs);
                CreateOptions();
            }

            Logger.Message($"Applying {_database.RecipeSpecs.Count} recipe specifications.");
            var cookingRecipeUsers = GetCookingRecipeUsers();

            foreach (var spec in _database.RecipeSpecs)
            {
                var recipe = recipes[spec.Name];
                spec.ApplyTo(recipe, cookingRecipeUsers);
            }

            _currentCache = _database.RecipeSpecs.FirstOrDefault(r => r.IsUnlocking);
        }

        public List<RecipeSpec> GetCurrentOptions()
        {
            return _database?.RecipeSpecs
                .Where(r => r.IsOption)
                .ToList() ?? _emptySpecs;
        }

        public void SetChoice(string specLabel)
        {
            if (specLabel == null) { return; }
            var chosen = _database.RecipeSpecs.FirstOrDefault(s => s.Label == specLabel);
            if (chosen == null) { return; }

            foreach (var s in _database.RecipeSpecs.Where(s => s.IsOption))
            {
                s.IsOption = false;
                s.WasOption = true;
            }

            chosen.IsUnlocking = true;
            _currentCache = chosen;
        }

        public bool IsChoicePending()
        {
            return _database.RecipeSpecs.Any(r => r.IsOption);
        }

        public RecipeSpec GetCurrent()
        {
            return _currentCache;
        }

        public RecipeSpec GetByLabel(string label)
        {
            if (label == null) { return null; }

            RecipeSpec spec = null;
            _database?.ByLabel.TryGetValue(label, out spec);
            return spec;
        }

        public override void Tick(int currentTick)
        {
            if (_currentCache == null) { return; }

            _currentCache.Progress += 1;
            if (_currentCache.Progress < _currentCache.MaxProgress) { return; }

            _currentCache.Progress = _currentCache.MaxProgress;
            _currentCache.IsUnlocking = false;
            _currentCache.IsUnlocked = true;

            var cookingRecipeUsers = GetCookingRecipeUsers();
            var recipe = DefDatabase<RecipeDef>.GetNamed(_currentCache.Name);
            cookingRecipeUsers.ForEach(u => u.AllRecipes.Add(recipe));

            string message = $"{_currentCache.Label} has been unlocked.";
            Find.LetterStack.ReceiveLetter("Recipe Unlocked", message, LetterType.Good);

            _currentCache = null;

            CreateOptions();
        }

        private void CreateOptions()
        {
            var next = _database.RecipeSpecs
                .Where(s => !s.WasOption)
                .OrderBy(s => s.Ingredients.Count)
                .Take(5)
                .ToList();

            int baseProgress = Materia.Settings.ProgressPerTurn[_database.Turn - 1];

            foreach (var s in next)
            {
                s.IsOption = true;
                s.MaxProgress = baseProgress + _random.Next(-60000, 60000);
            }

            if (next.Count > 0) { Find.LetterStack.ReceiveLetter("New Recipe Choices", "You can choose a new recipe on the materia tab.", LetterType.Good); }

            _database.Turn++;
        }

        private void RemoveNormalCookingRecipes()
        {
            Logger.Message("Removing all normal cooking recipes.");

            var cookingRecipes = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.products != null && r.workSkill == SkillDefOf.Cooking)
                .Where(r => !r.defName.StartsWith("MateriaRecipe") && r.defName != "CookMealSimple" && r.defName != "MakePemmican")
                .ToHashSet();

            var cookingIngredients = cookingRecipes
                .Where(r => r.ingredients != null)
                .SelectMany(r => r.ingredients)
                .Where(i => i.filter?.AllowedThingDefs != null)
                .SelectMany(i => i.filter.AllowedThingDefs)
                .ToHashSet();

            var mealProducts = cookingRecipes
                .SelectMany(r => r.products)
                .Where(p => p.thingDef != null)
                .Select(p => p.thingDef)
                .Where(t => !cookingIngredients.Contains(t))
                .ToHashSet();

            var recipesToRemove = cookingRecipes
                .Where(r => r.products.Any(p => mealProducts.Contains(p.thingDef)))
                .ToHashSet();

            var buildings = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => t.AllRecipes?.Count > 0);

            foreach (var building in buildings)
            {
                foreach (var recipe in building.AllRecipes.ToList())
                {
                    if (!recipesToRemove.Contains(recipe)) { continue; }
                    building.AllRecipes.Remove(recipe);
                    recipe.recipeUsers?.Clear();
                }
            }
        }

        private List<ThingDef> GetCookingRecipeUsers()
        {
            var electricStove = DefDatabase<ThingDef>.GetNamed("ElectricStove");
            var fueledStove = DefDatabase<ThingDef>.GetNamed("FueledStove");
            return new List<ThingDef> { electricStove, fueledStove };
        }
    }
}
