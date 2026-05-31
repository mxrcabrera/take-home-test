using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fundo.Applications.WebApi
{
    // NOTE: This project uses the legacy Startup.cs pattern (WebHost.CreateDefaultBuilder) instead of the modern minimal hosting pattern (WebApplication.CreateBuilder).
    // The minimal hosting pattern introduced in .NET 6 is the recommended approach for new projects as it reduces boilerplate and provides better performance.
    // However, migrating to minimal hosting requires updating integration tests to use the new WebApplicationFactory pattern and may break existing test configurations.
    // For this take-home test, the legacy pattern was kept to ensure all tests pass without extensive refactoring.
    // In a production environment, this should be migrated to the minimal hosting pattern.
    public class Program
    {
        private const int MaxDatabaseRetries = 10;
        private const int RetryDelaySeconds = 5;

        public static void Main(string[] args)
        {
            try
            {
                var host = CreateWebHostBuilder(args).Build();
                bool dbInitialized = false;

                for (int retry = 0; retry < MaxDatabaseRetries; retry++)
                {
                    try
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var services = scope.ServiceProvider;
                            var context = services.GetRequiredService<LoanDbContext>();

                            context.Database.CanConnect();
                            DbInitializer.Initialize(context);
                            dbInitialized = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = host.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "Database connection attempt {Retry}/{MaxRetries} failed. Retrying in {Delay} seconds...",
                            retry + 1, MaxDatabaseRetries, RetryDelaySeconds);

                        if (retry < MaxDatabaseRetries - 1)
                        {
                            System.Threading.Tasks.Task.Delay(RetryDelaySeconds * 1000).Wait();
                        }
                        else
                        {
                            logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts.", MaxDatabaseRetries);
                            throw;
                        }
                    }
                }

                if (!dbInitialized)
                {
                    throw new InvalidOperationException("Failed to initialize database after multiple retries.");
                }

                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled WebApi exception: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Application shutting down.");
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
