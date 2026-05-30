using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class CreateLoanDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(100, 1000000, ErrorMessage = "Amount must be between 100 and 1,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Applicant name is required")]
        [MinLength(2, ErrorMessage = "Applicant name must be at least 2 characters")]
        [MaxLength(200, ErrorMessage = "Applicant name cannot exceed 200 characters")]
        public string ApplicantName { get; set; } = string.Empty;
    }
}
