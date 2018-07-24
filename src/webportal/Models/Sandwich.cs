using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webportal.NetworkModels;

namespace webportal.Models
{
    public class Sandwich
    {
        IngredientsList ingredientsList;
        public int[] InputIngredientsArray { get; set; }
        public string[] OutputIngredientsArray { get; set; }

        public Sandwich(int[] input)
        {
            InputIngredientsArray = input;
        }

        public void ConvertToString()
        {
            ingredientsList = new IngredientsList();
            OutputIngredientsArray = new string[InputIngredientsArray.Length];

            for (int i = 0; i < InputIngredientsArray.Length; i++)
            {
                if (InputIngredientsArray[i] == 1)
                {
                    OutputIngredientsArray[i] = ingredientsList.Ingredients[i];
                }
            }
        }
    }
}
