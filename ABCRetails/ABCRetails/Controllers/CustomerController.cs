using System.Text.Json;
using ABCRetails.Models;
using ABCRetailsFunctions.Models;
using ABCRetailsFunctions.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetails.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(IFunctionsApi api, ILogger<CustomerController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            try
            {
                // NOTE: Assuming IFunctionsApi.GetCustomersAsync() returns List<CustomerDto> based on Index and Details methods
                var customerDtos = await _api.GetCustomersAsync();

                // Convert CustomerDto list to Customer list for the view
                var customers = customerDtos.Select(dto => new Customer
                {
                    RowKey = dto.Id,
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Username = dto.Username,
                    Email = dto.Email,
                    ShippingAddress = dto.ShippingAddress
                }).ToList();

                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers");
                TempData["Error"] = $"Error loading customers: {ex.Message}";
                return View(new List<Customer>());
            }
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // NOTE: Assuming IFunctionsApi.GetCustomerAsync() returns CustomerDto? based on this method
                var customerDto = await _api.GetCustomerAsync(id);
                if (customerDto == null)
                {
                    return NotFound();
                }

                // Convert CustomerDto to Customer for the view
                var customer = new Customer
                {
                    RowKey = customerDto.Id,
                    Name = customerDto.Name,
                    Surname = customerDto.Surname,
                    Username = customerDto.Username,
                    Email = customerDto.Email,
                    ShippingAddress = customerDto.ShippingAddress
                };

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer details for {CustomerId}", id);
                TempData["Error"] = $"Error loading customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            var customer = new Customer
            {
                // Initialize with new RowKey
                RowKey = Guid.NewGuid().ToString()
            };
            return View(customer);
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // Remove validation for Azure Table properties that are auto-generated
            ModelState.Remove("PartitionKey");
            ModelState.Remove("Timestamp");
            ModelState.Remove("ETag");
            ModelState.Remove("CustomerId");
            ModelState.Remove("Id");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("OrdersCount");

            if (!ModelState.IsValid)
            {
                // Detailed logging from the first snippet
                _logger.LogWarning("Model validation failed for customer creation");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                }
                return View(customer);
            }

            try
            {
                // Ensure RowKey is set (as per both snippets)
                if (string.IsNullOrEmpty(customer.RowKey))
                {
                    customer.RowKey = Guid.NewGuid().ToString();
                }

                // Convert Customer to CustomerDto
                var customerDto = new CustomerDto(
                    Id: customer.RowKey,
                    Name: customer.Name ?? string.Empty,
                    Surname: customer.Surname ?? string.Empty,
                    Username: customer.Username ?? string.Empty,
                    Email: customer.Email ?? string.Empty,
                    ShippingAddress: customer.ShippingAddress ?? string.Empty
                );

                // Logging before API call (from the first snippet)
                _logger.LogInformation("Attempting to create customer: {CustomerName}", customerDto.Name);
                await _api.CreateCustomerAsync(customerDto);

                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", customer.Name);
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");

                // Log inner exception if it exists (from the first snippet)
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception while creating customer");
                    ModelState.AddModelError("", $"Inner error: {ex.InnerException.Message}");
                }

                return View(customer);
            }
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            try
            {
                var customerDto = await _api.GetCustomerAsync(id);
                if (customerDto == null)
                {
                    return NotFound();
                }

                // Convert CustomerDto to Customer for the view
                var customer = new Customer
                {
                    RowKey = customerDto.Id,
                    Name = customerDto.Name,
                    Surname = customerDto.Surname,
                    Username = customerDto.Username,
                    Email = customerDto.Email,
                    ShippingAddress = customerDto.ShippingAddress
                };

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer for edit: {CustomerId}", id);
                TempData["Error"] = $"Error loading customer for edit: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer)
        {
            if (string.IsNullOrEmpty(id) || id != customer.RowKey)
            {
                return NotFound();
            }

            // Remove validation for Azure Table properties that are auto-generated
            ModelState.Remove("PartitionKey");
            ModelState.Remove("Timestamp");
            ModelState.Remove("ETag");
            ModelState.Remove("CustomerId");
            ModelState.Remove("Id");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("OrdersCount");

            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                // Convert Customer to CustomerDto
                var customerDto = new CustomerDto(
                    Id: customer.RowKey,
                    Name: customer.Name ?? string.Empty,
                    Surname: customer.Surname ?? string.Empty,
                    Username: customer.Username ?? string.Empty,
                    Email: customer.Email ?? string.Empty,
                    ShippingAddress: customer.ShippingAddress ?? string.Empty
                );

                await _api.UpdateCustomerAsync(customer.RowKey, customerDto);
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(customer);
            }
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var customerDto = await _api.GetCustomerAsync(id);
                if (customerDto == null)
                {
                    return NotFound();
                }

                // Convert CustomerDto to Customer for the view
                var customer = new Customer
                {
                    RowKey = customerDto.Id,
                    Name = customerDto.Name,
                    Surname = customerDto.Surname,
                    Username = customerDto.Username,
                    Email = customerDto.Email,
                    ShippingAddress = customerDto.ShippingAddress
                };

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer for deletion: {CustomerId}", id);
                TempData["Error"] = $"Error loading customer for deletion: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _api.DeleteCustomerAsync(id);
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Customer/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid JSON file.";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Please select a JSON file.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var customers = JsonSerializer.Deserialize<List<Customer>>(content, options);

                if (customers != null && customers.Any())
                {
                    int successCount = 0;
                    int errorCount = 0;

                    foreach (var customer in customers)
                    {
                        try
                        {
                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(customer.Name) ||
                                string.IsNullOrWhiteSpace(customer.Surname) ||
                                string.IsNullOrWhiteSpace(customer.Username) ||
                                string.IsNullOrWhiteSpace(customer.Email) ||
                                string.IsNullOrWhiteSpace(customer.ShippingAddress))
                            {
                                errorCount++;
                                continue;
                            }

                            // Ensure RowKey is set
                            if (string.IsNullOrEmpty(customer.RowKey))
                            {
                                customer.RowKey = Guid.NewGuid().ToString();
                            }

                            // Convert Customer to CustomerDto
                            var customerDto = new CustomerDto(
                                Id: customer.RowKey,
                                Name: customer.Name ?? string.Empty,
                                Surname: customer.Surname ?? string.Empty,
                                Username: customer.Username ?? string.Empty,
                                Email: customer.Email ?? string.Empty,
                                ShippingAddress: customer.ShippingAddress ?? string.Empty
                            );

                            await _api.CreateCustomerAsync(customerDto);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error uploading customer: {CustomerName}", customer.Name);
                            errorCount++;
                        }
                    }

                    if (successCount > 0)
                    {
                        TempData["Success"] = $"Successfully uploaded {successCount} customers.";
                        if (errorCount > 0)
                        {
                            TempData["Warning"] = $"{errorCount} customers failed to upload due to validation errors.";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "No customers were successfully uploaded. Please check your file format.";
                    }
                }
                else
                {
                    TempData["Error"] = "No valid customer data found in the file.";
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in uploaded file");
                TempData["Error"] = "Invalid JSON format. Please check your file.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading customer file");
                TempData["Error"] = $"Error uploading file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Download
        public async Task<IActionResult> Download()
        {
            try
            {
                var customerDtos = await _api.GetCustomersAsync();

                // Convert CustomerDto list to Customer list for download
                var customers = customerDtos.Select(dto => new Customer
                {
                    RowKey = dto.Id,
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Username = dto.Username,
                    Email = dto.Email,
                    ShippingAddress = dto.ShippingAddress
                }).ToList();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(customers, options);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                var fileName = $"customers_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading customers");
                TempData["Error"] = $"Error downloading customers: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}