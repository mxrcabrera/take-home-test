using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class LoginDto
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 50;
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 100;

        [Required(ErrorMessage = "Username is required")]
        [MinLength(MinUsernameLength, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(MaxUsernameLength, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(MinPasswordLength, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(MaxPasswordLength, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
