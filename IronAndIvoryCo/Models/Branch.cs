using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public class Branch
    {
        public int BranchId { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime OpeningTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime ClosingTime { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
