using Verse;

namespace Materia.Components
{
    public class MateriaProgressProp : CompProperties
    {
        public float Value;

        public MateriaProgressProp()
        {
            compClass = typeof(MateriaProgressComp);
        }
    }
}
