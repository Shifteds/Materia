using HugsLib;
using HugsLib.Utils;
using Materia.Models;

namespace Materia
{
    public class MateriaMod : ModBase
    {
        private MateriaDatabase _database;

        public override string ModIdentifier => "Materia";

        public override void WorldLoaded()
        {
            if (!ModIsActive) { return; }

            Logger.Message("Definitions loaded.");

            _database = UtilityWorldObjectManager.GetUtilityWorldObject<MateriaDatabase>();
            if (_database.Recipes.Count != 0)
            {
                Logger.Message($"Loaded {_database.Recipes.Count} recipes from the save file.");
                return;
            }

            var recipeGen = new RecipesGen();
            recipeGen.Populate(_database);
        }
    }
}
