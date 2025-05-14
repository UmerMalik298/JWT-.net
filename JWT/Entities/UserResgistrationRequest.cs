using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JWT.Entities
{
    public class UserResgistrationRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }



        [Required, MinLength(6)]

        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
