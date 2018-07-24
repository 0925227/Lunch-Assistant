using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webportal.NetworkModels
{
    public class IngredientsList
    {
        public List<string> Ingredients { get; private set; }
        public List<int> IngredientsWithCalo { get; set; }

        public IngredientsList()
        {
            Ingredients = new List<string>
            {
                "wit",
                "bruin",
                "BBQ",
                "ketchup",
                "sla",
                "tomaat",
                "ham",
                "kaas",
                "ui",
                "ei"
            };
            IngredientsWithCalo = new List<int>
            {
                250,
                220,
                17,
                11,
                1,
                5,
                50,
                120,
                3,
                68
            };
        }
    }
}
