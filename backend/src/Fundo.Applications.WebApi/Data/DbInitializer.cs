using BCrypt.Net;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Fundo.Applications.WebApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(LoanDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            var testUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12),
                Email = "admin@loanmanagement.com",
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
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Loan
                {
                    Amount = 15000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Jane Smith",
                    Status = "paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new Loan
                {
                    Amount = 50000.00m,
                    CurrentBalance = 32500.00m,
                    ApplicantName = "Robert Johnson",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new Loan
                {
                    Amount = 10000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Emily Williams",
                    Status = "paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Loan
                {
                    Amount = 75000.00m,
                    CurrentBalance = 72000.00m,
                    ApplicantName = "Michael Brown",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };

            context.Loans.AddRange(loans);
            context.SaveChanges();
        }
    }
}
