using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Controllers;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Unit
{
    public class LoansControllerTests
    {
        private readonly Mock<ILoanService> _loanServiceMock;
        private readonly Mock<ILogger<LoansController>> _loggerMock;
        private readonly LoansController _controller;

        public LoansControllerTests()
        {
            _loanServiceMock = new Mock<ILoanService>();
            _loggerMock = new Mock<ILogger<LoansController>>();
            _controller = new LoansController(_loanServiceMock.Object);
        }

        [Fact]
        public async Task GetLoans_ReturnsEmptyList_WhenNoLoansExist()
        {
            _loanServiceMock.Setup(x => x.GetAllLoansAsync()).ReturnsAsync(Array.Empty<Loan>());

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
                Id = 1,
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = LoanConstants.StatusActive,
                CreatedAt = DateTime.UtcNow
            };
            _loanServiceMock.Setup(x => x.GetAllLoansAsync()).ReturnsAsync(new[] { loan });

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
            _loanServiceMock.Setup(x => x.GetLoanByIdAsync(999)).ReturnsAsync((Loan?)null);

            var result = await _controller.GetLoan(999);

            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLoan_ReturnsLoan_WhenLoanExists()
        {
            var loan = new Loan
            {
                Id = 1,
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test Applicant",
                Status = LoanConstants.StatusActive,
                CreatedAt = DateTime.UtcNow
            };
            _loanServiceMock.Setup(x => x.GetLoanByIdAsync(1)).ReturnsAsync(loan);

            var result = await _controller.GetLoan(1);

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
            var createdLoan = new Loan
            {
                Id = 1,
                Amount = 15000m,
                CurrentBalance = 15000m,
                ApplicantName = "New Applicant",
                Status = LoanConstants.StatusActive,
                CreatedAt = DateTime.UtcNow
            };
            _loanServiceMock.Setup(x => x.CreateLoanAsync(createLoanDto)).ReturnsAsync(createdLoan);

            var result = await _controller.CreateLoan(createLoanDto);

            Assert.NotNull(result);
            var createdResult = Assert.IsType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(createdResult.Value);
            Assert.Equal("New Applicant", loanDto.ApplicantName);
            Assert.Equal(15000m, loanDto.Amount);
            Assert.Equal(15000m, loanDto.CurrentBalance);
            Assert.Equal(LoanConstants.StatusActive, loanDto.Status);
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
            _loanServiceMock.Setup(x => x.MakePaymentAsync(999, 1000m)).ReturnsAsync((Loan?)null);

            var result = await _controller.MakePayment(999, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenLoanIsNotActive()
        {
            var paymentDto = new PaymentDto { Amount = 1000m };
            _loanServiceMock.Setup(x => x.MakePaymentAsync(1, 1000m))
                .ThrowsAsync(new InvalidOperationException("Loan is not active"));

            var result = await _controller.MakePayment(1, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenPaymentExceedsBalance()
        {
            var paymentDto = new PaymentDto { Amount = 6000m };
            _loanServiceMock.Setup(x => x.MakePaymentAsync(1, 6000m))
                .ThrowsAsync(new InvalidOperationException("Payment exceeds balance"));

            var result = await _controller.MakePayment(1, paymentDto);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_UpdatesBalance_WhenValid()
        {
            var paymentDto = new PaymentDto { Amount = 2000m };
            var updatedLoan = new Loan
            {
                Id = 1,
                Amount = 10000m,
                CurrentBalance = 3000m,
                ApplicantName = "Test Applicant",
                Status = LoanConstants.StatusActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _loanServiceMock.Setup(x => x.MakePaymentAsync(1, 2000m)).ReturnsAsync(updatedLoan);

            var result = await _controller.MakePayment(1, paymentDto);

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(okResult.Value);
            Assert.Equal(3000m, loanDto.CurrentBalance);
            Assert.Equal(LoanConstants.StatusActive, loanDto.Status);
            Assert.NotNull(loanDto.UpdatedAt);
        }

        [Fact]
        public async Task MakePayment_SetsStatusToPaid_WhenBalanceReachesZero()
        {
            var paymentDto = new PaymentDto { Amount = 5000m };
            var updatedLoan = new Loan
            {
                Id = 1,
                Amount = 10000m,
                CurrentBalance = 0m,
                ApplicantName = "Test Applicant",
                Status = LoanConstants.StatusPaid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _loanServiceMock.Setup(x => x.MakePaymentAsync(1, 5000m)).ReturnsAsync(updatedLoan);

            var result = await _controller.MakePayment(1, paymentDto);

            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var loanDto = Assert.IsType<LoanDto>(okResult.Value);
            Assert.Equal(0m, loanDto.CurrentBalance);
            Assert.Equal(LoanConstants.StatusPaid, loanDto.Status);
        }
    }
}
