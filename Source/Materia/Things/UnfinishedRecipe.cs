using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class UnfinishedRecipe : UnfinishedThing
    {
        private string _descriptionString;

        public List<string> Ingredients { get; set; } = new List<string>();

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            var materiaStation = BoundBill?.billStack?.billGiver as MateriaStation;
            if (materiaStation == null)
            {
                Log.Error($"[Materia] Unfinished recipe created by {Creator?.NameStringShort} could not find its origin materia station.");
                return;
            }

            Ingredients = materiaStation.ConsumeIngredients()
                .Select(d => d.defName)
                .ToList();

            string s = Ingredients.Aggregate("Ingredients: ", (current, ing) => current + $"{ing}, ");
            _descriptionString = s.Substring(0, s.Length - 2);

            Log.Message($"[Materia] Created unfinished recipe with: {_descriptionString}.");
        }

        public override string GetDescription()
        {
            return _descriptionString;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            RecipeRegistry.OnUnfinishedRecipeDone(Creator, Ingredients);
            base.Destroy(mode);
        }
    }
}
