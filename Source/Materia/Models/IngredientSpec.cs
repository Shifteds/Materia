using Verse;

namespace Materia.Models
{
    public class IngredientSpec : IExposable
    {
        public string Name;
        public int Amount;
        public bool AnyMeat, AnyPlant;

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref Name, nameof(Name));
            Scribe_Values.LookValue(ref Amount, nameof(Amount));
            Scribe_Values.LookValue(ref AnyMeat, nameof(AnyMeat));
            Scribe_Values.LookValue(ref AnyPlant, nameof(AnyPlant));
        }
    }
}
