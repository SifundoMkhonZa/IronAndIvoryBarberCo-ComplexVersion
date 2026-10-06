using IronAndIvoryCo.Models.IronAndIvoryCo.Models;
using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public class Admin : Person
    {
        [Required]
        public string AccessCode { get; set; } = "ADMIN123";
        public ICollection<Staff> StaffMembers { get; set; } = new HashSet<Staff>();
    }
}
