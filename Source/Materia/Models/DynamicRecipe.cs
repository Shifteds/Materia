using Verse;

namespace Materia.Models
{
    public class DynamicRecipe : IExposable
    {
        public string Name;

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref Name, nameof(Name));
        }
    }
}
