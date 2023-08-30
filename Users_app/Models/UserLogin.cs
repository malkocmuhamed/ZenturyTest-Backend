using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users_app.Models
{
    public class UserLogin
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }   

        public  string? UserName { get; set; }

        public  string? Password { get; set; }

        public bool? isLoginSuccessful { get; set; }

        public DateTime? LoginDateTime { get; set; }

        // Navigation property for the related User
        public User? User { get; set; }

    }
}
