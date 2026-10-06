using IronAndIvoryCo.Models.Enums;
using IronAndIvoryCo.Models.IronAndIvoryCo.Models;
using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public class Receptionist : Staff
    {
        [Required]
        public Shift Shift { get; set; }

        public string DeskNumber { get; set; } = "R01";
    }
}
