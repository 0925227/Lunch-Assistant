using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webportal.Models.NeuralNetworkViewModels
{
    public class SandwichIngredientItemViewModel
    {
        public string Name { get; set; }
        public bool Checked { get; set; }

        public SandwichIngredientItemViewModel(string name, bool isChecked)
        {
            Name = name;
            Checked = isChecked;
        }
    }
}
