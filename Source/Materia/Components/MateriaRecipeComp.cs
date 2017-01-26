using Verse;

namespace Materia
{
    public class MateriaRecipeComp : ThingComp
    {
        private string _label;

        public RecipeSpec RecipeSpec { get; set; }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            RecipeSpec = RecipeRegistry.GetLastUnfinishedRecipe();
            if (RecipeSpec?.Name != null) { _label = $"{RecipeSpec.Name} Recipe"; }
        }

        public override string TransformLabel(string label)
        {
            return _label ?? label;
        }

        public override string GetDescriptionPart()
        {
            return RecipeSpec?.Description ?? string.Empty;
        }

        public override string CompInspectStringExtra()
        {
            return RecipeSpec?.IngredientDescription ?? string.Empty;
        }
    }
}
