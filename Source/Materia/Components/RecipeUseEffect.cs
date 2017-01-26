using RimWorld;
using Verse;

namespace Materia
{
    public class RecipeUseEffect : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            var recipeComp = parent.TryGetComp<MateriaRecipeComp>();
            var spec = recipeComp?.RecipeSpec;

            if (spec == null)
            {
                Messages.Message("There was nothing to learn from this recipe.", MessageSound.Negative);
                parent.Destroy();
                return;
            }

            if (spec.Learned)
            {
                Messages.Message($"{spec.Name} has already been learned.", MessageSound.Standard);
            }
            else
            {
                Messages.Message($"Learned {spec.Name}.", MessageSound.Benefit);
                RecipeRegistry.Learn(spec);
            }

            parent.Destroy();
        }
    }
}
