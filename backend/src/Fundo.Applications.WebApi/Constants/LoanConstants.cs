namespace Fundo.Applications.WebApi.Constants
{
    public static class LoanConstants
    {
        public const string StatusActive = "active";
        public const string StatusPaid = "paid";
        
        public const string ErrorLoanNotFound = "Loan not found.";
        public const string ErrorLoanNotActive = "Loan is not active. Cannot make payment.";
        public const string ErrorPaymentExceedsBalance = "Payment amount exceeds current balance.";
    }
}
