using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IronAndIvoryCo.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; } = null!;

        public int BarberId { get; set; }
        [ValidateNever]
        public Barber Barber { get; set; } = null!;

        public int AppointmentId { get; set; }
        [ValidateNever]
        public Appointment Appointment { get; set; } = null!;
    }
}
