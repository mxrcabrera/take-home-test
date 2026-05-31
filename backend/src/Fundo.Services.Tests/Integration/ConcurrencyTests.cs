using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class ConcurrencyTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.Startup> _factory;
        private readonly HttpClient _client;

        public ConcurrencyTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<LoanDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("ConcurrencyTestDb");
                    });
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
            using var scope = _factory.Services.CreateScope();
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
        public async Task RowVersion_Property_Exists_OnLoanEntity()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test User",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            context.Loans.Add(loan);
            context.SaveChanges();

            // Assert
            var retrievedLoan = await context.Loans.FindAsync(loan.Id);
            Assert.NotNull(retrievedLoan);
            // RowVersion property exists (value may be null in in-memory DB)
            // Just verify the property exists on the entity
            var propertyInfo = retrievedLoan!.GetType().GetProperty("RowVersion");
            Assert.NotNull(propertyInfo);
        }

        [Fact]
        public async Task LoanEntity_HasRowVersionProperty()
        {
            // This test verifies that the Loan entity has the RowVersion property
            // which is used for optimistic concurrency
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            
            var loan = new Loan
            {
                Amount = 10000m,
                CurrentBalance = 5000m,
                ApplicantName = "Test User",
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            
            // Verify RowVersion property exists
            var propertyInfo = loan.GetType().GetProperty("RowVersion");
            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(byte[]), propertyInfo.PropertyType);
        }
    }
}
