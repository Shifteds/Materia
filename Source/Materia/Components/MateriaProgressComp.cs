using Verse;

namespace Materia.Components
{
    public class MateriaProgressComp : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            var matProps = (MateriaProgressProp)props;
            MateriaMod.Instance.AddProgress(matProps.Value);
        }
    }
}
