using Verse;

namespace Materia.Models
{
    public class HediffSpec : IExposable
    {
        [Unsaved]
        private string _descriptionCache;

        public string Name, Label, StatType;
        public float Value;
        public bool IsPositive;
        public int Tier;

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref Name, nameof(Name));
            Scribe_Values.LookValue(ref Label, nameof(Label));
            Scribe_Values.LookValue(ref Value, nameof(Value));
            Scribe_Values.LookValue(ref Tier, nameof(Tier));
            Scribe_Values.LookValue(ref StatType, nameof(StatType));
            Scribe_Values.LookValue(ref IsPositive, nameof(IsPositive));
        }

        public string GetDescription()
        {
            return _descriptionCache ?? (_descriptionCache = $"{Label}: {Value * 100:0.00}%");
        }
    }
}
