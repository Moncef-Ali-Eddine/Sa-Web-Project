using System.ComponentModel.DataAnnotations;

namespace test.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only letters and numbers.")]
        public string Username { get; set; }

        [Required]
        public string Password_hash { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }
    }
}
