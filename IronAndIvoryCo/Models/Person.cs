using System.ComponentModel.DataAnnotations;

namespace IronAndIvoryCo.Models
{
    public abstract class Person
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        private string _email;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Email is required");

                if (!value.Contains("@") || !value.Contains("."))
                    throw new Exception("Email must contain @ and.");

                _email = value;
            }
        }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
