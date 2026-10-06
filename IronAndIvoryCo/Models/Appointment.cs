using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using IronAndIvoryCo.Models.Enums;

namespace IronAndIvoryCo.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly Time { get; set; }

        [Required]
        public AppointmentStatus AppointmentStatus { get; set; } = AppointmentStatus.Pending;

        public string? Note { get; set; } = "";

        public bool IsPaid { get; set; } = false;

        [Range(0, 10000)]
        public decimal AmountPaid { get; set; } = 0m;

        public int CustomerId { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; } = null!;

        public int BarberId { get; set; }
        [ValidateNever]
        public Barber Barber { get; set; } = null!;

        public int ServiceId { get; set; }
        [ValidateNever]
        public Service Service { get; set; } = null!;

        public int? ReceptionistId { get; set; }
        [ValidateNever]
        public Receptionist? Receptionist { get; set; }

        public int BranchId { get; set; }
        [ValidateNever]
        public Branch Branch { get; set; } = null!;

        [ValidateNever]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [ValidateNever]
        public Review? Review { get; set; }

        // For old views that use Model.Payment
        [ValidateNever]
        public Payment? Payment => Payments.FirstOrDefault();
    }
}