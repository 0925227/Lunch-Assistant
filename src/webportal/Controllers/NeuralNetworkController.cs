using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeuralNetwork.Helpers;
using NeuralNetwork.NetworkModels;
using webportal.Data;
using webportal.Models;
using webportal.Models.NeuralNetworkViewModels;
using webportal.NetworkModels;
using webportal.Services;
using Combinatorics.Collections;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace webportal.Controllers
{
    public class NeuralNetworkController : Controller
    {
        // MVC stuff
        private readonly IFileWriterService _fileWriterService;
        private readonly ImportHelper _importHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        protected readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        // Neural Network stuff
        private static int _numInputParameters;
        private static int _numHiddenLayers;
        private static int[] _hiddenNeurons;
        private static int _numOutputParameters;
        private static Network _network;
        private static List<DataSet> _dataSets;
        IngredientsList sandwichIngredients;

        public NeuralNetworkController(IFileWriterService fileWriterService, ImportHelper importHelper, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _fileWriterService = fileWriterService;
            _importHelper = importHelper;
            _userManager = userManager;
            _dbContext = context;
            _logger = logger;
            _numInputParameters = 10;
            _numHiddenLayers = 1;
            _hiddenNeurons = new int[] { 2 };
            _numOutputParameters = 1;
            sandwichIngredients = new IngredientsList();
        }

        public IActionResult Index()
        { 
            return View();
        }


        // Process the order
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(NeuralNetworkIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Intent for accessing profile
                if (model.SandwichInputIngredients.Contains("Profiel") || model.SandwichInputIngredients.Contains("profiel"))
                {
                    // Intent for reading profile
                    if (model.SandwichInputIngredients.Contains("lezen") || 
                        model.SandwichInputIngredients.Contains("inzien") || 
                        model.SandwichInputIngredients.Contains("weten") || 
                        model.SandwichInputIngredients.Contains("horen") ||
                        model.SandwichInputIngredients.Contains("opvragen"))                        
                    {
                        return Redirect("Profile");
                    }
                    // Intent for editing profile
                    else if (model.SandwichInputIngredients.Contains("invoeren")  || 
                             model.SandwichInputIngredients.Contains("invullen")  || 
                             model.SandwichInputIngredients.Contains("aanmaken")  ||
                             model.SandwichInputIngredients.Contains("aanpassen") ||
                             model.SandwichInputIngredients.Contains("bewerken"))
                    {
                        return Redirect("EnterProfile");
                    }
                }

                var v = ParseIngredients(model.SandwichInputIngredients);
                ViewBag.OriginalString = model.SandwichInputIngredients;
                ViewBag.Order = OutputString(v);
            }
            return View();
        }

        // Get the users profile 
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await GetCurrentUser();

            string health = "";

            if (IsUserHealty(user))
            {
                health += "gezond";
            }
            else
            {
                health += "ongezond";
            }

            string output = $"Je bent een {user.Sex}, je bent {user.Age} jaar oud, je weegt {user.Weigth} kilo en je bent {health} bezig!" ;

            model.OutputSpeech = output;

            return View(model);
        }

        // Enter profile
        public async Task<IActionResult> EnterProfile()
        {
            return View();
        }

        // Post profile
        [HttpPost]
        public async Task<IActionResult> EnterProfile(EnterProfileViewModel model)
        {
            var user = await GetCurrentUser();

            int number;

            var profile = model.ProfileInput.Split(' ');
            for (int i = 0; i < profile.Length; i++)
            {
                if(profile[i].ToLower() == "man")
                {
                    user.Sex = "man";
                } else if (Int32.TryParse(profile[i], out number))
                {
                    if (number < 50)
                    {
                        user.Age = number;
                    } else
                    {
                        user.Weigth = number;
                    }
                }
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // Confirm the order
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(ConfirmOrderViewModel model)
        {       
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUser();

                if (model.Answer.Contains("Ja") || model.Answer.Contains("ja"))
                {
                    // Network stuff

                    if (!(System.IO.File.Exists($@"wwwroot\data\{user.Email}.txt")))
                    {
                        _network = new Network(_numInputParameters, _hiddenNeurons, _numOutputParameters);
                    }

                    _network = ImportHelper.ImportNetwork(user.Email);

                    if (_network == null)
                    {
                        _logger.LogError("Something went wrong while importing the network");
                    }

                    _numInputParameters = _network.InputLayer.Count;
                    _hiddenNeurons = new int[_network.HiddenLayers.Count];
                    _numOutputParameters = _network.OutputLayer.Count;

                    var v = ParseIngredients(model.OriginalOrder);

                    var newDatasets = new List<DataSet>();
                    
                    double[] expectedResult = new double[] { 1 };
                    newDatasets.Add(new DataSet(v.Select(Convert.ToDouble).ToArray(), expectedResult));
                    _dataSets = newDatasets;

                    for (int i = 0; i < 100; i++)
                    {
                        _network.Train(newDatasets, 1);
                    }                                                    

                    ExportHelper.ExportNetwork(_network, user.Email);

                    var sandwich = new Order { Email = user.Email, OrderLine = ConvertIntToString(v), DateTime = DateTime.Now };
                    _dbContext.Orders.Add(sandwich);
                    _dbContext.SaveChanges();

                    return Redirect("Goodbye");
                }
                else
                {
                    if (model.OriginalOrder.Contains("suggestie"))
                    {
                        return Redirect("Suggestion");
                    }
                }
            }
            return Redirect("Index");
        }

        // Function to make api call to get food information
        public void GetFoodFromApi(string food)
        {
            var request = (HttpWebRequest)WebRequest.Create($@"https://api.nutritionix.com/v1_1/search/{food}?results=0%3A20&cal_min=0&cal_max=50000&fields=item_name%2Cbrand_name%2Citem_id%2Cbrand_id%2Cnf_calories&appId=0602074e&appKey=22046cff82be7747387084f357c49415");

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            var response = (HttpWebResponse)request.GetResponse();

            string content = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(stream))
                {
                    content = sr.ReadToEnd();
                }
            }

            var releases = JsonConvert.DeserializeObject(content);
                        
        }

        // Say goodbye
        public IActionResult Goodbye()
        {
            return View();
        }

        // Make suggestion
        public async Task<IActionResult> Suggestion()
        {
            var user = await GetCurrentUser();

            if (!(System.IO.File.Exists($@"wwwroot\data\{user.Email}.txt")))
            {
                return Redirect("Index");
            }

            _network = ImportHelper.ImportNetwork(user.Email);

            if (_network == null)
            {
                _logger.LogError("Something went wrong while importing the network");
            }

            List<double[]> suggestions = MakeSuggestion(user);

            if (suggestions.Count() < 1)
            {
                return Redirect("Index");
            }

            Random r = new Random();

            string sandwich = ConvertIntToSuggestionSandwich(suggestions.Skip(r.Next(1, suggestions.Count)).FirstOrDefault());

            ViewBag.Suggestion = sandwich;

            return View();
        }

        // Helper functions

        private async Task<ApplicationUser> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

        public int[] ParseIngredients(string inputFromSpeech)
        {
            string[] _inputFromSpeech = inputFromSpeech.Split(' ');

            int[] newSandwich = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach (var word in _inputFromSpeech)
            {
                for (int i = 0; i < sandwichIngredients.Ingredients.Count; i++)
                {
                    if (word == sandwichIngredients.Ingredients[i])
                    {
                        newSandwich[i] = 1;
                    }
                }

                if (newSandwich[0] == 0 && newSandwich[1] == 0)
                {
                    newSandwich[1] = 1;
                }
            }
            return newSandwich;
        }

        public string OutputString(int[] input)
        {
            string output = "Je wilt een ";
            if (input[0] == 1)
            {
                output += "wit";
            } else
            {
                output += "bruin";
            }
            output += " broodje met: ";
            for (int i = 2; i < input.Count(); i++)
            {
                if (input[i] == 1)
                {
                    output += sandwichIngredients.Ingredients[i] + "en ";
                }
            }
            output = output.Substring(0, output.Length - 3) + " Klopt dit?";
            return output;
        }

        public string ConvertIntToSuggestionSandwich(double[] input)
        {

            string output = "Je suggestie voor vandaag is een ";
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
            output = output.Substring(0, output.Length - 3) + "Vind je dit lekker?";

            return output;
        }

        public string ConvertIntToString(int[] i)
        {
            string output = $"{i[0]},{i[1]},{i[2]},{i[3]},{i[4]},{i[5]},{i[6]},{i[7]},{i[8]},{i[9]}";
            return output;
        }

        public double[] ConvertStringToInt(string i)
        {
            double[] sandwich = new double[10];
            var v = i.Split(',');           

            int cnt = 0;
            foreach (var item in v)
            {
                var convertedSandwich = Double.Parse(item);
                sandwich[cnt] = convertedSandwich;
                cnt++;
            }
            return sandwich;
        }
        
        public List<double[]> MakeSuggestion(ApplicationUser user)
        {
            var integers = new List<int> { 1, 0 };

            var everyPossibleVariation = new Variations<int>(integers, 10, GenerateOption.WithRepetition);

            List<(double[] sandwich, double[] probability)> highProbability = new List<(double[] sandwich, double[] probability)>();

            foreach (var sandwich in everyPossibleVariation)
            {
                var v = _network.Compute(sandwich.Select(Convert.ToDouble).ToArray());

                //Check if lekker
                if (v[0] > 0.8)
                {
                    highProbability.Add((sandwich.Select(Convert.ToDouble).ToArray(), v));
                }
            }

            //Order by highest probability
            List<(double[] sandwich, double[] probability)> highProbabilityOrdered = new List<(double[] sandwich, double[] probability)>();
            highProbabilityOrdered = highProbability;
            highProbabilityOrdered.OrderBy(x => x.probability);

            //Sandwich that is healty and also 
            List<double[]> suggestionSandwich = new List<double[]>();
            foreach (var (sandwich, probability) in highProbabilityOrdered)
            {
                //_logger.LogInformation("Suggestionsandwich called");
                if (IsHealty(sandwich, user) ) // Also add && if exists in database and IfIsGoodSuggestion
                {
                    suggestionSandwich.Add(sandwich);
                }
            }
            return suggestionSandwich;
        }

        public bool IsUserHealty(ApplicationUser user)
        {
            var d = _dbContext.Orders.Where(x => x.Email == user.Email).Select(x => x.OrderLine); // Make it recent
            int CaloriesInLastMonth = 0;
            foreach (var orderline in d)
            {
                var z = ConvertStringToInt(orderline);
                for (int i = 0; i < z.Count(); i++)
                {
                    if (z[i] == 1)
                    {
                        CaloriesInLastMonth += sandwichIngredients.IngredientsWithCalo[i];
                    }
                }
            }

            //GetFoodFromApi(string food)
            if (CaloriesInLastMonth > 8000)
            {
                return false;

            }
            return true;
        }

        public bool IsHealty(double[] sandwich, ApplicationUser user)
        {
            int calories = 0;
            for (int i = 0; i < sandwich.Count(); i++)
            {
                if (sandwich[i] == 1)
                {
                    calories += sandwichIngredients.IngredientsWithCalo[i];
                }
            }

            // Check recent orders
            var d = _dbContext.Orders.Where(x => x.Email == user.Email).Select(x => x.OrderLine); // Make it recent
            int CaloriesInLastMonth = 0;
            foreach (var orderline in d)
            {
                var z = ConvertStringToInt(orderline);
                for (int i = 0; i < z.Count(); i++)
                {
                    if (z[i] == 1)
                    {
                        CaloriesInLastMonth += sandwichIngredients.IngredientsWithCalo[i];
                    }
                }
            }

            if (CaloriesInLastMonth > 8000)
            {
                if (calories > 400)
                {
                    return false;
                }
            } else
            {
                if (calories > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }
}