using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using IronAndIvoryCo.Models.Enums;

namespace IronAndIvoryCo.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Amount must be > 0")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public paymentMethod paymentMethod { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int? SaleId { get; set; }
        [ValidateNever]
        public Sale? Sale { get; set; }

        public int? AppointmentId { get; set; }
        [ValidateNever]
        public Appointment? Appointment { get; set; }
    }
}
