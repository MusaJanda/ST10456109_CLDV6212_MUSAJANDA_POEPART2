using ABCRetails.Models;
using ABCRetailsFunctions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ABCRetails.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi functionsApi, ILogger<ProductController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["Error"] = "Error loading products. Please try again.";
                return View(new List<Product>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            _logger.LogInformation("Create product POST method started");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                }
                return View(product);
            }

            try
            {
                if (product.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than R0.00");
                    return View(product);
                }

                _logger.LogInformation("Creating product: {ProductName}, Price: {Price}, Stock: {Stock}",
                    product.ProductName, product.Price, product.StockAvailable);

                // Use the Model-based method instead of DTO
                var createdProduct = await _functionsApi.CreateProductAsync(product, imageFile);

                _logger.LogInformation("Product created successfully, redirecting to index");
                TempData["Success"] = $"Product {createdProduct.ProductName} created successfully with price {createdProduct.Price:C}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception while creating product");
                }

                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _functionsApi.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            try
            {
                var updatedProduct = await _functionsApi.UpdateProductAsync(product.RowKey, product, imageFile);
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _functionsApi.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {Message}", ex.Message);
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}