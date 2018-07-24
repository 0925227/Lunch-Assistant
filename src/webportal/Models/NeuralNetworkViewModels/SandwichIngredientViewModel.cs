using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webportal.NetworkModels;

namespace webportal.Models.NeuralNetworkViewModels
{
    public class SandwichIngredientViewModel
    {
        IngredientsList sandwichIngredients;
        public List<SandwichIngredientItemViewModel> Ingredients { get; set; } = new List<SandwichIngredientItemViewModel>();

        public SandwichIngredientViewModel()
        {
            sandwichIngredients = new IngredientsList();

            var items = sandwichIngredients.Ingredients;

            foreach (var item in items)
            {
                Ingredients.Add(new SandwichIngredientItemViewModel(item, false));
            }
        }
    }
}
