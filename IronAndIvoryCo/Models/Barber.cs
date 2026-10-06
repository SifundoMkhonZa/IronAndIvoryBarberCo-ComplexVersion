using IronAndIvoryCo.Models.Enums;
using IronAndIvoryCo.Models.IronAndIvoryCo.Models;

namespace IronAndIvoryCo.Models
{
    public class Barber : Staff
    {
        public Speciality Speciality { get; set; }

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}