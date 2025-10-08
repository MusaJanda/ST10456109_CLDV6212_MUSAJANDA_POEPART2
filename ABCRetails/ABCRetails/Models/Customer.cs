using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABCRetails.Models
{
    // The Customer class serves as an entity for Azure Table Storage.
    public class Customer : ITableEntity
    {
        // ITableEntity properties for Azure Table Storage
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Core Identity and Display Properties

        [Required]
        [Display(Name = "First Name")] // Merged to be more descriptive than "Name"
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")] // Merged to be more descriptive than "Surname"
        public string Surname { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required] // Added Required based on second block, useful for customer sign-up
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Contact/Address Properties

        [Display(Name = "Phone")] // From first block
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Shipping Address")] // From second block
        public string ShippingAddress { get; set; } = string.Empty;

        // Derived/Internal Properties (from both blocks)

        // CustomerId (Alias for RowKey - from second block)
        [Display(Name = "Customer ID")]
        public string CustomerId => RowKey;

        // Id (Alias for RowKey - from first block)
        public string Id => RowKey;

        // Internal/Tracking Properties (from second block)

        public string? Status { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
        public int OrdersCount { get; internal set; }
    }
}