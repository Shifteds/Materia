using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using Verse;

namespace Materia.Models
{
    public class MateriaDatabase : UtilityWorldObject
    {
        [Unsaved]
        private Dictionary<string, RecipeSpec> _byLabel;

        public List<RecipeSpec> RecipeSpecs = new List<RecipeSpec>();

        public int Turn = 1;

        public Dictionary<string, RecipeSpec> ByLabel
        {
            get { return _byLabel ?? (_byLabel = RecipeSpecs.ToDictionary(r => r.Label, r => r)); }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue(ref Turn, nameof(Turn));
            Scribe_Collections.LookList(ref RecipeSpecs, nameof(RecipeSpecs), LookMode.Deep);
        }
    }
}
