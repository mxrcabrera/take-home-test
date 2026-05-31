using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class CreateLoanDto
    {
        private const double MinLoanAmount = 100;
        private const double MaxLoanAmount = 1000000;
        private const int MinApplicantNameLength = 2;
        private const int MaxApplicantNameLength = 200;

        [Required(ErrorMessage = "Amount is required")]
        [Range(MinLoanAmount, MaxLoanAmount, ErrorMessage = "Amount must be between 100 and 1,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Applicant name is required")]
        [MinLength(MinApplicantNameLength, ErrorMessage = "Applicant name must be at least 2 characters")]
        [MaxLength(MaxApplicantNameLength, ErrorMessage = "Applicant name cannot exceed 200 characters")]
        public string ApplicantName { get; set; } = string.Empty;
    }
}
