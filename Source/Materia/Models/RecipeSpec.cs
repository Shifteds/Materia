using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace Materia.Models
{
    public class RecipeSpec : IExposable
    {
        private static ThingFilter _meatFilter;
        public string Name, Label, Description, ProductLabel, ProductDescription;
        public bool IsOption, WasOption, IsUnlocked, IsUnlocking;
        public float Nutrition, Mass, MarketValue, WorkToMake, Progress, MaxProgress;
        public int Yield, DaysToRot, Skill;

        public List<IngredientSpec> Ingredients = new List<IngredientSpec>();

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref Name, nameof(Name));
            Scribe_Values.LookValue(ref Label, nameof(Label));
            Scribe_Values.LookValue(ref Description, nameof(Description));
            Scribe_Values.LookValue(ref ProductLabel, nameof(ProductLabel));
            Scribe_Values.LookValue(ref ProductDescription, nameof(ProductDescription));

            Scribe_Values.LookValue(ref IsUnlocked, nameof(IsUnlocked));
            Scribe_Values.LookValue(ref IsUnlocking, nameof(IsUnlocking));
            Scribe_Values.LookValue(ref IsOption, nameof(IsOption));
            Scribe_Values.LookValue(ref WasOption, nameof(WasOption));

            Scribe_Values.LookValue(ref Nutrition, nameof(Nutrition));
            Scribe_Values.LookValue(ref Mass, nameof(Mass));
            Scribe_Values.LookValue(ref MarketValue, nameof(MarketValue));
            Scribe_Values.LookValue(ref WorkToMake, nameof(WorkToMake));
            Scribe_Values.LookValue(ref Yield, nameof(Yield));
            Scribe_Values.LookValue(ref DaysToRot, nameof(DaysToRot));
            Scribe_Values.LookValue(ref Progress, nameof(Progress));
            Scribe_Values.LookValue(ref MaxProgress, nameof(MaxProgress));

            Scribe_Collections.LookList(ref Ingredients, nameof(Ingredients), LookMode.Deep);
        }

        public void ApplyTo(RecipeDef recipe, List<ThingDef> users)
        {
            recipe.label = Label;
            recipe.description = Description;
            recipe.products.First().count = Yield;

            recipe.skillRequirements = new List<SkillRequirement>
            {
                new SkillRequirement {minLevel = Skill, skill = SkillDefOf.Cooking}
            };

            // Ingredients
            var ingredients = new List<IngredientCount>();
            foreach (var ing in Ingredients)
            {
                var ingCount = new IngredientCount();

                if (ing.AnyMeat) { ingCount.filter = MeatFilter; }
                else
                {
                    var thing = DefDatabase<ThingDef>.GetNamed(ing.Name);
                    ingCount.filter = new ThingFilter();
                    ingCount.filter.SetAllow(thing, true);
                }

                ingCount.SetBaseCount(ing.Amount);
                ingredients.Add(ingCount);
            }

            recipe.ingredients = ingredients;

            // Product
            var product = recipe.products.First().thingDef;
            product.label = ProductLabel;

            typeof(Def).GetField("cachedLabelCap", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(product, ProductLabel);

            product.description = ProductDescription;
            product.ingestible.nutrition = Nutrition;
            product.statBases.First(s => s.stat == StatDefOf.Mass).value = Mass;
            product.statBases.First(s => s.stat == StatDefOf.MarketValue).value = MarketValue;
            product.statBases.First(s => s.stat == StatDefOf.WorkToMake).value = WorkToMake;
            product.comps.OfType<CompProperties_Rottable>().First().daysToRotStart = DaysToRot;

            // Label isn't set without setting the cache here for some reason.
            GenLabel.ThingLabel(product, null);

            // Add it to production buildings.
            if (!IsUnlocked) { return; }

            recipe.recipeUsers = users.ToList();
            users.ForEach(u => u.AllRecipes.Add(recipe));
        }

        private static ThingFilter MeatFilter
        {
            get
            {
                return _meatFilter ?? (_meatFilter = DefDatabase<RecipeDef>.GetNamed("MateriaMeatRecipe").ingredients
                           .First().filter);
            }
        }
    }
}
