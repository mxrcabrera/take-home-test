using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services {
    public class LoanService : ILoanService {
        private readonly LoanDbContext _context;
        private const decimal ZeroBalance = 0m;

        public LoanService(LoanDbContext context) => _context = context;

        public async Task<Loan[]> GetAllLoansAsync() => 
            await _context.Loans.AsNoTracking().ToArrayAsync();

        public async Task<Loan?> MakePaymentAsync(int id, decimal amount) {
            if (amount <= 0) 
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null || loan.Status == LoanConstants.StatusPaid) 
                return null;

            loan.CurrentBalance -= amount;
            
            if (loan.CurrentBalance <= ZeroBalance) {
                loan.CurrentBalance = ZeroBalance;
                loan.Status = LoanConstants.StatusPaid;
            }
            
            loan.UpdatedAt = DateTime.UtcNow;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw new Exception("Conflict: The record was modified by another process.");
            }
            return loan;
        }

        public async Task<Loan> CreateLoanAsync(CreateLoanDto dto) {
            var loan = new Loan {
                Amount = dto.Amount,
                CurrentBalance = dto.Amount,
                ApplicantName = dto.ApplicantName,
                Status = LoanConstants.StatusActive
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<Loan?> GetLoanByIdAsync(int id) => await _context.Loans.FindAsync(id);
    }
}