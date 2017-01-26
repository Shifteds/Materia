using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Materia
{
    public class MateriaStation : Building_WorkTable
    {
        private List<IntVec3> _cachedAdjCellsCardinal;

        private IEnumerable<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                return _cachedAdjCellsCardinal ?? (_cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal(this)
                    .Where(c => c.InBounds(Map))
                    .ToList());
            }
        }

        private List<ThingDef> FindIngredients()
        {
            var ingredients = new List<ThingDef>();

            foreach (var cell in AdjCellsCardinalInBounds)
            {
                var thingsInCell = cell.GetThingList(Map);

                var hopper = thingsInCell.FirstOrDefault(i => i.def == ThingDefOf.Hopper);
                if (hopper == null) { continue; }

                var ingredient = thingsInCell.FirstOrDefault(i => i != hopper);
                if (ingredient != null) { ingredients.Add(ingredient.def); }
            }

            return ingredients;
        }

        public List<ThingDef> ConsumeIngredients()
        {
            return FindIngredients();
        }
    }
}
