using BCrypt.Net;
using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Fundo.Applications.WebApi.Data
{
    public static class DbInitializer
    {
        private const int BcryptWorkFactor = 12;

        public static void Initialize(LoanDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            // SECURITY NOTE: Default admin credentials for development/testing only
            // In production, admin credentials should be:
            // - Created via secure admin setup process
            // - Loaded from environment variables
            // - Never hardcoded in source control
            // Consider implementing a first-run setup wizard for production
            var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin123";
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@loanmanagement.com";

            var testUser = new User
            {
                Username = adminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: BcryptWorkFactor),
                Email = adminEmail,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(testUser);
            context.SaveChanges();

            if (context.Loans.Any())
            {
                return;
            }

            var loans = new[]
            {
                new Loan
                {
                    Amount = 25000.00m,
                    CurrentBalance = 18750.00m,
                    ApplicantName = "John Doe",
                    Status = LoanConstants.StatusActive,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Loan
                {
                    Amount = 15000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Jane Smith",
                    Status = LoanConstants.StatusPaid,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new Loan
                {
                    Amount = 50000.00m,
                    CurrentBalance = 32500.00m,
                    ApplicantName = "Robert Johnson",
                    Status = LoanConstants.StatusActive,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new Loan
                {
                    Amount = 10000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Emily Williams",
                    Status = LoanConstants.StatusPaid,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Loan
                {
                    Amount = 75000.00m,
                    CurrentBalance = 72000.00m,
                    ApplicantName = "Michael Brown",
                    Status = LoanConstants.StatusActive,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };

            context.Loans.AddRange(loans);
            context.SaveChanges();
        }
    }
}
