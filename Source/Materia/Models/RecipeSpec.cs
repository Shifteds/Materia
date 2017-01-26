using System.Collections.Generic;

namespace Materia
{
    public class RecipeSpec
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IngredientDescription { get; set; }
        public List<string> Ingredients { get; set; } = new List<string>();
        public bool Learned { get; set; }
    }
}
