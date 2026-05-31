using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services
{
    public class LoanService : ILoanService
    {
        private const decimal ZeroBalance = 0m;

        private readonly LoanDbContext _context;

        public LoanService(LoanDbContext context)
        {
            _context = context;
        }

        public async Task<Loan> CreateLoanAsync(CreateLoanDto createLoanDto)
        {
            var loan = new Loan
            {
                Amount = createLoanDto.Amount,
                CurrentBalance = createLoanDto.Amount,
                ApplicantName = createLoanDto.ApplicantName,
                Status = LoanConstants.StatusActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<Loan?> GetLoanByIdAsync(int id)
        {
            return await _context.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Loan[]> GetAllLoansAsync()
        {
            return await _context.Loans.AsNoTracking().ToArrayAsync();
        }

        public async Task<Loan?> MakePaymentAsync(int id, decimal amount)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return null;
            }

            if (loan.Status != LoanConstants.StatusActive)
            {
                throw new InvalidOperationException(LoanConstants.ErrorLoanNotActive);
            }

            if (amount > loan.CurrentBalance)
            {
                throw new InvalidOperationException(LoanConstants.ErrorPaymentExceedsBalance);
            }

            loan.CurrentBalance -= amount;
            loan.UpdatedAt = DateTime.UtcNow;

            if (loan.CurrentBalance == ZeroBalance)
            {
                loan.Status = LoanConstants.StatusPaid;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("The loan was modified by another process. Please try again.");
            }

            return loan;
        }
    }
}
