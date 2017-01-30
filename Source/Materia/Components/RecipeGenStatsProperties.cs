using Verse;

namespace Materia.Components
{
    public class RecipeGenStats : HediffCompProperties
    {
        public int Tier;
        public float Deviation, Value;
        public bool IsPositive;

        public RecipeGenStats()
        {
            compClass = typeof(RecipeGenStatsComp);
        }
    }
}
