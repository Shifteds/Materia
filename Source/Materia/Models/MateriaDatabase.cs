using System.Collections.Generic;
using HugsLib.Utils;
using Verse;

namespace Materia.Models
{
    public class MateriaDatabase : UtilityWorldObject
    {
        public List<DynamicRecipe> Recipes = new List<DynamicRecipe>();

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.LookList(ref Recipes, nameof(Recipes), LookMode.Deep);
        }
    }
}
