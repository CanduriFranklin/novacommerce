using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Domain
{
    public class Product(string name, string description, decimal price, int initialStock)
    {
        [Key]
        public Guid ProductId { get; init; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; private set; } = name;

        [StringLength(500)]
        public string Description { get; private set; } = description;

        [Range(0, 1000000)]
        public decimal Price { get; private set; } = price;

        [Range(0, int.MaxValue)]
        public int Stock { get; private set; } = initialStock;

        public string ImageUrl { get; private set; }

        public bool IsActive { get; private set; } = true;

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; private set; }

        // Private constructor for EF Core
        private Product() : this(string.Empty, string.Empty, 0, 0) { }

        public void UpdateDetails(string name, string description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name cannot be empty.", nameof(name));
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            Name = name;
            Description = description;
            Price = price;
            SetUpdated();
        }

        public void UpdateStock(int quantityChange, string reason)
        {
            if (Stock + quantityChange < 0)
            {
                throw new InvalidOperationException("Stock cannot be negative.");
            }
            Stock += quantityChange;
            // Here you would typically also create a StockMovement record
            SetUpdated();
        }

        public void SetImageUrl(string imageUrl)
        {
            ImageUrl = imageUrl;
            SetUpdated();
        }

        public void Activate()
        {
            IsActive = true;
            SetUpdated();
        }

        public void Deactivate()
        {
            IsActive = false;
            SetUpdated();
        }

        private void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
