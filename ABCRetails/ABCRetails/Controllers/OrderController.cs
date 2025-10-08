using ABCRetails.Models;
using ABCRetails.Models.ViewModels;
using ABCRetailsFunctions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetails.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IFunctionsApi functionsApi, ILogger<OrderController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all orders in Index");
                TempData["Error"] = "Error loading orders. Please try again.";
                return View(new List<Order>());
            }
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var order = await _functionsApi.GetOrderAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order Details requested for non-existent ID: {Id}", id);
                    return NotFound();
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for ID: {Id}", id);
                TempData["Error"] = "Error loading order details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new OrderCreateViewModel
                {
                    OrderDate = DateTime.Today,
                    Status = "Submitted"
                };

                await PopulateDropdowns(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create order page");
                TempData["Error"] = "Error loading order form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            _logger.LogInformation("Creating order - Customer: {CustomerId}, Product: {ProductId}, Quantity: {Quantity}",
                model.CustomerId, model.ProductId, model.Quantity);

            // Re-populate dropdowns immediately if model state is invalid
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                // Validate that CustomerId and ProductId are selected
                if (string.IsNullOrEmpty(model.CustomerId))
                {
                    ModelState.AddModelError("CustomerId", "Please select a customer");
                }
                if (string.IsNullOrEmpty(model.ProductId))
                {
                    ModelState.AddModelError("ProductId", "Please select a product");
                }

                if (!ModelState.IsValid)
                {
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Use the Functions API to create the order
                var order = await _functionsApi.CreateOrderAsync(model.CustomerId, model.ProductId, model.Quantity);

                // Use the more descriptive order property for the TempData success message
                var orderIdProperty = order.RowKey;
                if (string.IsNullOrEmpty(orderIdProperty)) orderIdProperty = "Unknown";

                TempData["Success"] = $"Order {orderIdProperty} submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError(string.Empty, $"Error creating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var order = await _functionsApi.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit view for order ID: {Id}", id);
                TempData["Error"] = $"Error loading order for edit: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order order)
        {
            if (id != order.RowKey)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Focus on status update, assuming the model only allows status modification on edit
                    if (!string.IsNullOrEmpty(order.Status))
                    {
                        await _functionsApi.UpdateOrderStatusAsync(id, order.Status);
                        _logger.LogInformation("Order {Id} status updated to {Status} via Edit POST", id, order.Status);
                    }
                    else
                    {
                        // Handle update logic for other fields if necessary, or log a warning
                        _logger.LogWarning("Order {Id} submitted for edit, but no status change detected.", id);
                    }

                    TempData["Success"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order ID: {Id} via Edit POST", id);
                    ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                }
            }
            return View(order);
        }

        // POST: Order/UpdateOrderStatus (Utility action)
        public async Task<IActionResult> UpdateOrderStatus(string id, string newStatus)
        {
            try
            {
                await _functionsApi.UpdateOrderStatusAsync(id, newStatus);
                _logger.LogInformation("Order {Id} status updated to {NewStatus} via direct action", id, newStatus);
                TempData["Success"] = $"Order {id} status updated to {newStatus}!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for ID: {Id} to {NewStatus}", id, newStatus);
                TempData["Error"] = $"Error updating order status: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/CancelOrder (Utility action)
        [HttpPost]
        public async Task<IActionResult> CancelOrder(string id)
        {
            try
            {
                await _functionsApi.UpdateOrderStatusAsync(id, "Cancelled");
                _logger.LogInformation("Order {Id} successfully cancelled", id);
                TempData["Success"] = $"Order {id} has been cancelled.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order ID: {Id}", id);
                TempData["Error"] = $"Error cancelling order: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _functionsApi.DeleteOrderAsync(id);
                _logger.LogInformation("Order {Id} successfully deleted", id);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order ID: {Id}", id);
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // Private method to populate Customer and Product dropdowns
        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            try
            {
                var customers = await _functionsApi.GetCustomersAsync();
                var products = await _functionsApi.GetProductsAsync();

                // FIX: Use RowKey instead of Id for dropdown values
                model.Customers = customers?.Select(c => new Customer
                {
                    RowKey = c.RowKey, // This is what gets sent to the API
                    Name = c.Name,
                    Surname = c.Surname,
                    Username = c.Username
                }).ToList() ?? new List<Customer>();

                model.Products = products?.Select(p => new Product
                {
                    RowKey = p.RowKey, // This is what gets sent to the API
                    ProductName = p.ProductName,
                    Price = p.Price,
                    StockAvailable = p.StockAvailable
                }).ToList() ?? new List<Product>();

                _logger.LogInformation("Populated dropdowns - {CustomerCount} customers, {ProductCount} products",
                    model.Customers.Count, model.Products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error populating dropdowns");
                model.Customers = new List<Customer>();
                model.Products = new List<Product>();
            }
        }
    }
}