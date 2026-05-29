using Fundo.Applications.WebApi.Controllers;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit
{
    public class LoansControllerTests
    {
        private readonly LoanDbContext _context;
        private readonly LoansController _controller;

        public LoansControllerTests()
        {
            var options = new DbContextOptionsBuilder<LoanDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LoanDbContext(options);
            var loggerMock = new Mock<ILogger<LoansController>>();
            _controller = new LoansController(_context);
        }

        [Fact]
        public async Task GetLoans_ReturnsEmptyList_WhenNoLoansExist()
        {
            var result = await _controller.GetLoans();

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loans = Assert.IsType<System.Collections.Generic.List<LoanDto>>(okResult.Value);
            Assert.Empty(loans);
        }

        [Fact]
        public async Task GetLoans_ReturnsListOfLoans_WhenLoansExist()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _controller.GetLoans();

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loans = Assert.IsType<System.Collections.Generic.List<LoanDto>>(okResult.Value);
            Assert.Single(loans);
            Assert.Equal("Test Applicant", loans[0].ApplicantName);
        }

        [Fact]
        public async Task GetLoan_ReturnsNotFound_WhenLoanDoesNotExist()
        {
            var result = await _controller.GetLoan(999);

            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLoan_ReturnsLoan_WhenLoanExists()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _controller.GetLoan(loan.Id);

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(okResult.Value);
            Assert.Equal("Test Applicant", loanDto.ApplicantName);
            Assert.Equal(10000m, loanDto.Amount);
        }

        [Fact]
        public async Task CreateLoan_ReturnsCreatedLoan_WhenValid()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 15000m,
                ApplicantName = "New Applicant"
            };

            var result = await _controller.CreateLoan(createLoanDto);

            Assert.NotNull(result);
            var createdResult = Assert.IsType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(createdResult.Value);
            Assert.Equal("New Applicant", loanDto.ApplicantName);
            Assert.Equal(15000m, loanDto.Amount);
            Assert.Equal(15000m, loanDto.CurrentBalance);
            Assert.Equal("active", loanDto.Status);
        }

        [Fact]
        public async Task CreateLoan_ReturnsBadRequest_WhenAmountIsZero()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 0,
                ApplicantName = "Test Applicant"
            };

            _controller.ModelState.AddModelError("Amount", "Amount must be greater than 0");

            var result = await _controller.CreateLoan(createLoanDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ReturnsNotFound_WhenLoanDoesNotExist()
        {
            var paymentDto = new PaymentDto { Amount = 1000m };

            var result = await _controller.MakePayment(999, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenLoanIsNotActive()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 0m,
                ApplicantName = "Test Applicant",
                Status = "paid",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto { Amount = 1000m };

            var result = await _controller.MakePayment(loan.Id, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenPaymentExceedsBalance()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto { Amount = 6000m };

            var result = await _controller.MakePayment(loan.Id, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_UpdatesBalance_WhenValid()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto { Amount = 2000m };

            var result = await _controller.MakePayment(loan.Id, paymentDto);

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(okResult.Value);
            Assert.Equal(3000m, loanDto.CurrentBalance);
            Assert.Equal("active", loanDto.Status);
            Assert.NotNull(loanDto.UpdatedAt);
        }

        [Fact]
        public async Task MakePayment_SetsStatusToPaid_WhenBalanceReachesZero()
        {
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto { Amount = 5000m };

            var result = await _controller.MakePayment(loan.Id, paymentDto);

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(okResult.Value);
            Assert.Equal(0m, loanDto.CurrentBalance);
            Assert.Equal("paid", loanDto.Status);
        }
    }
}
