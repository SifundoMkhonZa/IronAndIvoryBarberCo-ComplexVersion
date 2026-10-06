using IronAndIvoryCo.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IronAndIvoryCo.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace IronAndIvoryCo.Models
    {
        public class Staff : Person
        {
            public StaffRole Role { get; set; }

            [Required]
            public int BranchId { get; set; }

            [ForeignKey("BranchId")]
            public Branch Branch { get; set; } = null!;

            public int? AdminId { get; set; }
            public Admin? Admin { get; set; }

            public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        }
    }
}
