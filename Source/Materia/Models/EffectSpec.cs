using Verse;

namespace Materia.Models
{
    public class EffectSpec : IExposable
    {
        public string StatName;
        public string HediffName;
        public string Label;
        public string Description;
        public float Value;
        public int Tier;
        public EffectCategory Category;
        public EffectType Type;

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref StatName, nameof(StatName));
            Scribe_Values.LookValue(ref HediffName, nameof(HediffName));
            Scribe_Values.LookValue(ref Label, nameof(Label));
            Scribe_Values.LookValue(ref Description, nameof(Description));
            Scribe_Values.LookValue(ref Value, nameof(Value));
            Scribe_Values.LookValue(ref Tier, nameof(Tier));
            Scribe_Values.LookValue(ref Category, nameof(Category));
            Scribe_Values.LookValue(ref Type, nameof(Type));
        }

        public EffectSpec Clone()
        {
            return (EffectSpec) MemberwiseClone();
        }
    }

    public enum EffectType
    {
        Stat,
        Capacity
    }

    public enum EffectCategory
    {
        Buff, 
        Debuff
    }
}
