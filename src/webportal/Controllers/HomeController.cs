using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeuralNetwork.Helpers;
using webportal.Data;
using webportal.Models;
using webportal.NetworkModels;
using webportal.Services;

namespace webportal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileWriterService _fileWriterService;
        private readonly ImportHelper _importHelper;
        protected readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;
        Sandwich _sandwich;
        IngredientsList sandwichIngredients;

        public HomeController(IFileWriterService fileWriterService, ImportHelper importHelper, ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _fileWriterService = fileWriterService;
            _importHelper = importHelper;
            _logger = logger;
            _dbContext = context;
            sandwichIngredients = new IngredientsList();
        }

        //public IActionResult Index()
        //{    
        //    return View();
        //}

        public IActionResult Index(PortalIndexViewModel model)
        {
            //var orders = _dbContext.Orders.Where(x => x.DateTime == DateTime.Today).ToList();

            var orders = _dbContext.Orders.ToList();

            //PortalIndexViewModel thingy = new PortalIndexViewModel();

            foreach (var item in orders)
            {
                var d = new IndexItemViewModel
                {
                    OrderLine = GenerateSandwichString(ConvertStringToInt(item.OrderLine)),
                    DateTime = item.DateTime,
                    Email = item.Email
                };
                model.items.Add(d);
            }

            return View(model);
        }

        public IActionResult Stock()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string GenerateSandwichString(int[] input)
        {

            string output = "";
            if (input[0] == 1)
            {
                output += "wit";
            }
            else
            {
                output += "bruin";
            }
            output += " broodje met ";
            for (int i = 2; i < input.Count(); i++)
            {
                if (input[i] == 1)
                {
                    output += sandwichIngredients.Ingredients[i] + " en ";
                }
            }

            return output.Substring(0, output.Length - 3);
        }

        public int[] ConvertStringToInt(string i)
        {
            int[] sandwich = new int[10];
            var v = i.Split(',');

            int cnt = 0;
            foreach (var item in v)
            {
                var convertedSandwich = Int32.Parse(item);
                sandwich[cnt] = convertedSandwich;
                cnt++;
            }
            return sandwich;
        }
    }

}
