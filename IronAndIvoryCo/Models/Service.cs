using IronAndIvoryCo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Http;

    public class Service
    {
        public int ServiceId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ServiceCategory ServiceCategory { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        private decimal _price { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get => _price; set { if (value <= 0) throw new Exception("Price must be > 0"); _price = value; } }

        private int _durationInMinutes;
        [Required]
        [Range(5, 300)]
        public int DurationInMinutes { get => _durationInMinutes; set { if (value < 5 || value > 300) throw new Exception("Duration must be between 5 and 300 minutes"); _durationInMinutes = value; } }

        public string? Description { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
