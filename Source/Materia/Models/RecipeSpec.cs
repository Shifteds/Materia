using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Materia.Components;
using RimWorld;
using Verse;

namespace Materia.Models
{
    public class RecipeSpec : IExposable
    {
        [Unsaved]
        private string _ingredientTextCache, _statsTextCache;

        private static ThingFilter _meatFilter;
        public string Name, Label, Description, ProductLabel, ProductDescription;
        public bool IsOption, WasOption, IsUnlocked, IsUnlocking;
        public float Nutrition, Mass, MarketValue, WorkToMake, Progress, MaxProgress, ProgressGain;
        public int Yield, DaysToRot, Skill, Tier;

        public List<EffectSpec> Effects = new List<EffectSpec>();

        public List<IngredientSpec> Ingredients = new List<IngredientSpec>();

        public string GetIngredientText()
        {
            return _ingredientTextCache ?? (_ingredientTextCache = CreateIngredientText());
        }

        public string GetStatsText()
        {
            return _statsTextCache ?? (_statsTextCache = CreateStatsText());
        }

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
            Scribe_Values.LookValue(ref Tier, nameof(Tier));

            Scribe_Collections.LookList(ref Ingredients, nameof(Ingredients), LookMode.Deep);
            Scribe_Collections.LookList(ref Effects, nameof(Effects), LookMode.Deep);
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

                if (ing.Name == "Meat") { ingCount.filter = MeatFilter; }
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
            product.comps.OfType<MateriaProgressProp>().First().Value = ProgressGain;

            // Hediffs
            product.ingestible.outcomeDoers = new List<IngestionOutcomeDoer>();
            foreach (var effect in Effects)
            {
                var hediff = DefDatabase<HediffDef>.GetNamed(effect.HediffName);
                hediff.label = effect.Label;
                hediff.stages = new List<HediffStage>();
                var stage = new HediffStage();
                hediff.stages.Add(stage);

                switch (effect.Type)
                {
                    case EffectType.Stat:
                        stage.statOffsets = new List<StatModifier>();
                        var stat = DefDatabase<StatDef>.GetNamed(effect.StatName);
                        stage.statOffsets.Add(new StatModifier { stat = stat, value = effect.Value });
                        break;
                    case EffectType.Capacity:
                        stage.capMods = new List<PawnCapacityModifier>();
                        var cap = DefDatabase<PawnCapacityDef>.GetNamed(effect.StatName);
                        stage.capMods.Add(new PawnCapacityModifier { capacity = cap, offset = effect.Value });
                        break;
                }

                product.ingestible.outcomeDoers.Add(new IngestionOutcomeDoer_GiveHediff{severity = 1.0f, hediffDef = hediff});
            }

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

        private string CreateIngredientText()
        {
            var text = new StringBuilder();
            foreach (var ing in Ingredients) { text.Append($"{ing.Label}: {ing.Amount}\n\n"); }
            return text.ToString();
        }

        private string CreateStatsText()
        {
            int ingredientCount = Ingredients.Sum(i => i.Amount);
            float effectiveNutrition = Yield * Nutrition / ingredientCount;

            var text = new StringBuilder();
            text.Append($"{nameof(Yield)}:  {Yield}\n\n");
            text.Append($"{nameof(Nutrition)}:  {Nutrition:0.00}\n\n");
            text.Append($"Nutrition Per Ingredient:  {effectiveNutrition:0.00}\n\n");
            text.Append($"Days To Rot:  {DaysToRot}\n\n");
            text.Append($"{nameof(Mass)}:  {Mass:0.00}\n\n");
            text.Append($"Market Value:  {MarketValue:0.00}\n\n");
            if (Skill > 0) { text.Append($"Cooking Skill:  {Skill}\n\n"); }

            return text.ToString();
        }
    }
}
