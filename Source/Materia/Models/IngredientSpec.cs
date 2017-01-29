using Verse;

namespace Materia.Models
{
    public class IngredientSpec : IExposable
    {
        public string Name, Label;
        public int Amount;

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref Name, nameof(Name));
            Scribe_Values.LookValue(ref Label, nameof(Label));
            Scribe_Values.LookValue(ref Amount, nameof(Amount));
        }
    }
}
