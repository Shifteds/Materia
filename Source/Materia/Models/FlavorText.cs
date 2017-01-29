using System.Collections.Generic;

namespace Materia.Models
{
    public class FlavorText
    {
        public string Name, Description;
        public HashSet<string> Ingredients = new HashSet<string>();
    }
}
