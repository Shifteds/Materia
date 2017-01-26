using HugsLib;

namespace Materia
{
    public class MateriaMod : ModBase
    {
        public override string ModIdentifier => "Materia";

        public override void DefsLoaded()
        {

        }

        public override void WorldLoaded()
        {
            if (!ModIsActive) { return; }

            RecipeRegistry.ClearRecipes();
        }
    }
}
