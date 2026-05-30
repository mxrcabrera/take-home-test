using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fundo.Applications.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateWebHostBuilder(args).Build();

                // Retry logic for database connection
                int maxRetries = 10;
                int retryDelaySeconds = 5;
                bool dbInitialized = false;

                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var services = scope.ServiceProvider;
                            var context = services.GetRequiredService<LoanDbContext>();
                            
                            // Try to connect to the database
                            context.Database.CanConnect();
                            
                            // If we get here, connection succeeded
                            DbInitializer.Initialize(context);
                            dbInitialized = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = host.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "Database connection attempt {Retry}/{MaxRetries} failed. Retrying in {Delay} seconds...", 
                            retry + 1, maxRetries, retryDelaySeconds);
                        
                        if (retry < maxRetries - 1)
                        {
                            System.Threading.Thread.Sleep(retryDelaySeconds * 1000);
                        }
                        else
                        {
                            logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts.", maxRetries);
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
