using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IronAndIvoryCo.Models
{
    public class Customer : Person
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public int LoyaltyPoints { get; set; }

        public string? ApplicationUserId { get; set; } // Link to AspNetUsers

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        // ONE side - no FKs
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

