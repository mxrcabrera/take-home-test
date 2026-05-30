using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("loans")]
    [ApiController]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            var loanDtos = loans.Select(MapToDto).ToList();

            return Ok(loanDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(loan));
        }

        [HttpPost]
        public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto createLoanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loan = await _loanService.CreateLoanAsync(createLoanDto);
                return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, MapToDto(loan));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/payment")]
        public async Task<ActionResult<LoanDto>> MakePayment(int id, PaymentDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loan = await _loanService.MakePaymentAsync(id, paymentDto.Amount);

                if (loan == null)
                {
                    return NotFound();
                }

                return Ok(MapToDto(loan));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static LoanDto MapToDto(Loan loan)
        {
            return new LoanDto
            {
                Id = loan.Id,
                Amount = loan.Amount,
                CurrentBalance = loan.CurrentBalance,
                ApplicantName = loan.ApplicantName,
                Status = loan.Status,
                CreatedAt = loan.CreatedAt,
                UpdatedAt = loan.UpdatedAt
            };
        }
    }
}