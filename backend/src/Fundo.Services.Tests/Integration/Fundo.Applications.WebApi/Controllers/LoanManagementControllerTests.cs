using BCrypt.Net;
using Fundo.Applications.WebApi.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;

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

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Jwt:Key", "TestSecretKeyForJWTTokenGeneration123456789"),
                        new KeyValuePair<string, string?>("Jwt:Issuer", "LoanManagementAPI"),
                        new KeyValuePair<string, string?>("Jwt:Audience", "LoanManagementClient")
                    });
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            
            context.Database.EnsureCreated();
            
            if (!context.Users.Any())
            {
                var testUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12),
                    Email = "admin@test.com",
                    CreatedAt = System.DateTime.UtcNow
                };
                context.Users.Add(testUser);
                context.SaveChanges();
            }

            var loginDto = new LoginDto { Username = "admin", Password = "admin123" };
            var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginDto);
            loginResponse.EnsureSuccessStatusCode();
            
            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var responseObj = System.Text.Json.JsonDocument.Parse(responseContent);
            return responseObj.RootElement.GetProperty("token").GetString() ?? string.Empty;
        }

        private void SetAuthHeader(string token)
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        [Fact]
        public async Task GetLoans_ReturnsUnauthorized_WithoutToken()
        {
            var response = await _client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetLoans_ReturnsOk_WithValidToken()
        {
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var response = await _client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ReturnsUnauthorized_WithoutToken()
        {
            var createLoanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Integration Test Applicant"
            };

            var response = await _client.PostAsJsonAsync("/loans", createLoanDto);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ReturnsCreated_WithValidToken()
        {
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createLoanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Integration Test Applicant"
            };

            var response = await _client.PostAsJsonAsync("/loans", createLoanDto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ReturnsOk_And_DecreasesBalance()
        {
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createLoanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(createdLoan);

            var paymentDto = new PaymentDto { Amount = 2000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(updatedLoan);
            Assert.Equal(8000m, updatedLoan.CurrentBalance);
        }

        [Fact]
        public async Task MakePayment_ReturnsBadRequest_WhenAmountExceedsBalance()
        {
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createLoanDto = new CreateLoanDto
            {
                Amount = 5000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(createdLoan);

            var paymentDto = new PaymentDto { Amount = 6000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, paymentResponse.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ChangesStatusToPaid_WhenBalanceReachesZero()
        {
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createLoanDto = new CreateLoanDto
            {
                Amount = 5000m,
                ApplicantName = "Test Applicant"
            };
            var createResponse = await _client.PostAsJsonAsync("/loans", createLoanDto);
            createResponse.EnsureSuccessStatusCode();
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(createdLoan);

            var paymentDto = new PaymentDto { Amount = 5000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(updatedLoan);
            Assert.Equal(0m, updatedLoan.CurrentBalance);
            Assert.Equal(LoanConstants.StatusPaid, updatedLoan.Status);
        }
    }
}
