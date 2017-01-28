using System.Collections.Generic;
using HugsLib.Utils;
using Verse;

namespace Materia.Models
{
    public class MateriaDatabase : UtilityWorldObject
    {
        public List<RecipeSpec> RecipeSpecs = new List<RecipeSpec>();

        public int Turn = 1;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref Turn, nameof(Turn));
            Scribe_Collections.LookList(ref RecipeSpecs, nameof(RecipeSpecs), LookMode.Deep);
        }
    }
}
