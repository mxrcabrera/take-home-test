using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Services.Tests.Integration
{
    public class LoansControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.Startup> _factory;

        public LoansControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<LoanDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetLoans_ReturnsOk()
        {
            var response = await _client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ReturnsCreated()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Integration Test Applicant"
            };

            var response = await _client.PostAsJsonAsync("/loans", createLoanDto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetLoan_ReturnsNotFound_WhenLoanDoesNotExist()
        {
            var response = await _client.GetAsync("/loans/999");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ReturnsNotFound_WhenLoanDoesNotExist()
        {
            var paymentDto = new PaymentDto { Amount = 1000m };

            var response = await _client.PostAsJsonAsync("/loans/999/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ReturnsOk_And_DecreasesBalance()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            var paymentDto = new PaymentDto { Amount = 2000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.Equal(8000m, updatedLoan.CurrentBalance);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenAmountExceedsBalance()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 5000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            var paymentDto = new PaymentDto { Amount = 6000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, paymentResponse.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ChangesStatusToPaid_WhenBalanceReachesZero()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 5000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            var paymentDto = new PaymentDto { Amount = 5000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.Equal(0m, updatedLoan.CurrentBalance);
            Assert.Equal("paid", updatedLoan.Status);
        }
    }
}
