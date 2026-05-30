using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class PaymentDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000, ErrorMessage = "Payment amount must be between 0.01 and 1,000,000")]
        public decimal Amount { get; set; }
    }
}
