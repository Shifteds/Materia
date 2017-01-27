using Materia.Models;

namespace Materia
{
    internal class RecipesGen
    {
        public void Populate(MateriaDatabase database)
        {
            Logger.Log(this, "Populating the database.");

            database.Recipes.Add(new DynamicRecipe{Name = "Something"});
        }
    }
}
