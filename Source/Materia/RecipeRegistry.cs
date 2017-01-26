using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Materia
{
    internal static class RecipeRegistry
    {
        private static Dictionary<string, RecipeSpec> _recipeCache = new Dictionary<string, RecipeSpec>();
        private static RecipeSpec _lastUnfinishedRecipe;

        // This method serves as a bridge between the UnfinishedRecipe thing and the product Recipe thing.
        public static void OnUnfinishedRecipeDone(Pawn creator, List<string> ingredients)
        {
            _lastUnfinishedRecipe = GetRecipe(creator.NameStringShort, ingredients);
        }

        public static RecipeSpec GetLastUnfinishedRecipe()
        {
            var next = _lastUnfinishedRecipe;
            _lastUnfinishedRecipe = null;

            Log.Message($"[Materia] Recipe item asked for {next?.Name}.");

            return next;
        }

        public static void SetRecipes(List<RecipeSpec> specs)
        {
            _recipeCache = specs.ToDictionary(s => s.Guid, s => s);
        }

        public static void ClearRecipes()
        {
            _recipeCache.Clear();
        }

        // A recipe's GUID is the ingredients sorted and concatenated. There can only be one recipe for the same exact ingredients.
        public static RecipeSpec GetRecipe(string creator, List<string> ingredients)
        {
            ingredients.Sort();
            string guid = ingredients.Aggregate("INGS", (current, ing) => current + $":{ing}");
            if (_recipeCache.TryGetValue(guid, out var spec)) { return spec; }

            spec = new RecipeSpec
            {
                Guid = guid,
                Name = GetRecipeName(creator, ingredients),
                Ingredients = ingredients,
                IngredientDescription = GetIngredientDescription(ingredients)
            };

            _recipeCache[guid] = spec;

            Log.Message($"[Materia] Created new recipe spec for {spec.Name}.");

            return spec;
        }

        public static void Learn(RecipeSpec spec)
        {
            if (spec == null) { return; }
            if (spec.Learned) { return; }

            spec.Learned = true;

            var genericMeal = DefDatabase<ThingDef>.GetNamed("MealSimple");
            var genericRecipe = DefDatabase<RecipeDef>.GetNamed("GenericMeal");

            var mealDef = new ThingDef
            {
                thingClass = genericMeal.thingClass,
                defName = spec.Name.Replace(" ", ""),
                label = spec.Name,
                description = spec.Description,
                graphicData = genericMeal.graphicData,
                statBases = genericMeal.statBases.ToList(),
                ingestible = genericMeal.ingestible,
                comps = genericMeal.comps.ToList(),
                selectable = genericMeal.selectable,
                useHitPoints = genericMeal.useHitPoints,
                category = genericMeal.category,
                stackLimit = genericMeal.stackLimit,
                tickerType = genericMeal.tickerType,
                altitudeLayer = genericMeal.altitudeLayer,
                socialPropernessMatters = genericMeal.socialPropernessMatters,
                thingCategories = genericMeal.thingCategories.ToList(),
                alwaysHaulable = genericMeal.alwaysHaulable,
                pathCost = genericMeal.pathCost,
                resourceReadoutPriority = genericMeal.resourceReadoutPriority,
                drawGUIOverlay = genericMeal.drawGUIOverlay
            };

            DefDatabase<ThingDef>.Add(mealDef);
            Log.Message($"[Materia] Added a new item: {mealDef.defName}");

            var recipeDef = new RecipeDef
            {
                defName = $"Make{spec.Name.Replace(" ", "")}",
                label = $"Make {spec.Name}",
                description = genericRecipe.description,
                jobString = genericRecipe.jobString,
                allowMixingIngredients = true,
                workSpeedStat = genericRecipe.workSpeedStat,
                workSkill = genericRecipe.workSkill,
                workAmount = 1000,
                effectWorking = genericRecipe.effectWorking,
                soundWorking = genericRecipe.soundWorking,
                products = new List<ThingCountClass> { new ThingCountClass(mealDef, 1) },
                recipeUsers = genericRecipe.recipeUsers?.ToList() ?? new List<ThingDef>()
            };

            // Ingredient selection. Not required since default is volume selector.
            //var genericRecipeGetter = typeof(RecipeDef).GetField("ingredientValueGetterClass").GetValue(genericRecipe) as Type;
            //if (genericRecipeGetter != null) { recipeDef.GetType().GetField("ingredientValueGetterClass").SetValue(recipeDef, genericRecipeGetter); }

            // Skill requirements.
            var skillRequirement = new SkillRequirement
            {
                minLevel = 1,
                skill = SkillDefOf.Cooking
            };

            recipeDef.skillRequirements = new List<SkillRequirement> {skillRequirement};

            // Ingredients and filters.
            var ingredientDefs = spec.Ingredients
                .Select(i => DefDatabase<ThingDef>.GetNamed(i))
                .Where(i => i != null)
                .ToList();

            Log.Message($"[Materia] Ingredients: {ingredientDefs.Count}");
            if (ingredientDefs.Count != spec.Ingredients.Count) { Log.Error("[Materia] One or more of the ingredients has no def."); }

            var ingredients = new List<IngredientCount>();
            var fixedIngredientFilter = new ThingFilter();

            foreach (var i in ingredientDefs)
            {
                var filter = new ThingFilter();
                filter.SetAllow(i, true);

                var count = new IngredientCount{filter = filter};
                count.SetBaseCount(10f);
                ingredients.Add(count);

                fixedIngredientFilter.SetAllow(i, true);
            }

            recipeDef.ingredients = ingredients;
            recipeDef.fixedIngredientFilter = genericRecipe.fixedIngredientFilter;
            recipeDef.defaultIngredientFilter = genericRecipe.defaultIngredientFilter;

            // Users
            var users = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.AllRecipes.Any(r => r == genericRecipe))
                .Union(recipeDef.recipeUsers)
                .ToList();

            recipeDef.recipeUsers = users;
            Log.Message($"[Materia] Found {recipeDef.recipeUsers.Count} users of the recipe.");

            DefDatabase<RecipeDef>.Add(recipeDef);
            Log.Message($"[Materia] Added a new recipe: {recipeDef.defName}");

            // Clear the recipes cache of all recipe users.
            // Cache is always repopulated if null when something uses public AllRecipes property.
            foreach (var user in users) { user.AllRecipes.Add(recipeDef); }
        }

        // We could get fancy in here. Maybe look at how Tales are used by the Art comp?
        private static string GetRecipeName(string creator, IList<string> ingredients)
        {
            string name = $"{creator}'s ";
            if (ingredients.Count >= 2) { name += $"{CleanIngName(ingredients[0])} & {CleanIngName(ingredients[1])}"; }
            else if (ingredients.Count >= 1) { name += $"{CleanIngName(ingredients[0])}"; }
            else { name += "Special"; }

            return name;
        }

        private static string CleanIngName(string name)
        {
            return name.Split('_').Where(p => p != "Meat").Aggregate((prev, next) => $"{prev} {next}");
        }

        private static string GetIngredientDescription(IEnumerable<string> ingredients)
        {
            string s = ingredients.Aggregate("Ingredients: ", (current, ing) => current + $"{ing}, ");
            return s.Substring(0, s.Length - 2);
        }
    }
}
