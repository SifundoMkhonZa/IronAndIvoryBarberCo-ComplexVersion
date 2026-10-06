using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        [Required]
        public string DayOfWeek { get; set; } = "Monday";
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int BarberId { get; set; }
        [ForeignKey("BarberId")]
        [ValidateNever]
        public Barber Barber { get; set; } = null!;
    }
}
