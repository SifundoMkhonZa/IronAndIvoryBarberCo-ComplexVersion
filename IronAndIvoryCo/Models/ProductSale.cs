using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public class ProductSale
    {
        public int ProductSaleId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal UnitPrice { get; set; } // price at time of sale

        // Relationships - BOTH sides are MANY
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int SaleId { get; set; }
        public Sale Sale { get; set; }
    }
}
