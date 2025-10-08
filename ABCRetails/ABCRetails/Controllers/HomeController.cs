using System.Diagnostics;
using System.Linq;
using ABCRetails.Models;
using ABCRetails.Models.ViewModels;
using ABCRetailsFunctions.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetails.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public HomeController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetProductsAsync();
                var customers = await _functionsApi.GetCustomersAsync();
                var orders = await _functionsApi.GetOrdersAsync();

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = products?.Take(5).Select(p => new Product
                    {
                        RowKey = p.RowKey,
                        Id = p.RowKey, // Now this will work since Id is settable
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Price = p.Price,
                        StockAvailable = p.StockAvailable,
                        ImageUrl = p.ImageUrl
                    }).ToList() ?? new List<Product>(),
                    ProductCount = products?.Count() ?? 0,
                    CustomerCount = customers?.Count() ?? 0,
                    OrderCount = orders?.Count() ?? 0
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error and return a safe view model
                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = new List<Product>(),
                    ProductCount = 0,
                    CustomerCount = 0,
                    OrderCount = 0
                };
                return View(viewModel);
            }
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            return View("ContactUs");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult InitializeStorage()
        {
            // The code for InitializeStorage is not provided in the document.
            // However, the document mentions its existence.
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}