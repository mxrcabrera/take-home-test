using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class PaymentDto
    {
        private const double MinPaymentAmount = 0.01;
        private const double MaxPaymentAmount = 1000000;

        [Required(ErrorMessage = "Amount is required")]
        [Range(MinPaymentAmount, MaxPaymentAmount, ErrorMessage = "Payment amount must be between 0.01 and 1,000,000")]
        public decimal Amount { get; set; }
    }
}
