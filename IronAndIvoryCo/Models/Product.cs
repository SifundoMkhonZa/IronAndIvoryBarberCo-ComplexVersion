using IronAndIvoryCo.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IronAndIvoryCo.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string? Category { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Range(0, 10000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000)]
        public int StockQuantity { get; set; }

        [Required]
        public string Brand { get; set; }

        public int BranchId { get; set; }

        [ValidateNever]
        public Branch Branch { get; set; }

        [ValidateNever]
        public ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();
    }
}
