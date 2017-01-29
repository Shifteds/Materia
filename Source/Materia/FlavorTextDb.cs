using System;
using System.Collections.Generic;
using System.Linq;
using Materia.Models;

namespace Materia
{
    public class FlavorTextDb
    {
        public static FlavorTextDb Instance { get; set; } = new FlavorTextDb();

        public List<FlavorText> All { get; set; } = new List<FlavorText>
        {
            new FlavorText
            {
                Name = "Ramsey's Beef Wellington",
                Description = "\"This beef is so raw, it's starting to eat the salad!\"",
                Ingredients = new HashSet<string>{"Flour", "Meat"}
            },
            new FlavorText
            {
                Name = "Yorkshire Tea",
                Description = "Let's have a proper brew.",
                Ingredients = new HashSet<string>{"Rawtea"}
            },
            new FlavorText
            {
                Name = "Twinnings",
                Description = "A rich, full-flavored superior tea blend.",
                Ingredients = new HashSet<string>{"Rawtea"}
            },
            new FlavorText
            {
                Name = "Disnof's Special",
                Description = "\"It's our biggest export.\"",
                Ingredients = new HashSet<string>{"Human_Meat"}
            },
            new FlavorText
            {
                Name = "School Cafeteria Mystery Meatballs",
                Description = "\"Sir, we had to make budget cuts on both public education and prisons.\"\n\"Don't worry, I know what to do.\"",
                Ingredients = new HashSet<string>{"Human_Meat"}
            },
            new FlavorText
            {
                Name = "Jamiro's Rice Dish",
                Description = "\"5/10 without rice. 7/10 with rice.\"",
                Ingredients = new HashSet<string>{"RawRice"}
            }
        };

        public Dictionary<string, List<FlavorText>> GetFlavorMap()
        {
            return All
                .SelectMany(f => f.Ingredients)
                .GroupBy(i => i)
                .Select(g => g.Key.ToLower())
                .ToDictionary(i => i, i => All.Where(a => a.Ingredients.Contains(i)).OrderBy(a => Guid.NewGuid()).ToList());
        }
    }
}
