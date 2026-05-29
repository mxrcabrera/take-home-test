using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services
{
    public interface ILoanService
    {
        Task<Loan> CreateLoanAsync(CreateLoanDto createLoanDto);
        Task<Loan?> GetLoanByIdAsync(int id);
        Task<Loan[]> GetAllLoansAsync();
        Task<Loan?> MakePaymentAsync(int id, decimal amount);
    }
}
