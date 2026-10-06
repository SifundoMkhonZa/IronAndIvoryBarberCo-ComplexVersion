using System.ComponentModel.DataAnnotations;
using IronAndIvoryCo.Models.Enums;
namespace IronAndIvoryCo.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public SaleStatus Status { get; set; } = SaleStatus.Pending;

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int? BranchId { get; set; }
        public Branch Branch { get; set; }

        public ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();

        // FIXED: Changed from single Payment? to collection
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}