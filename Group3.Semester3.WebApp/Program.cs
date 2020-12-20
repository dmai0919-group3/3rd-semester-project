using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Group3.Semester3.WebApp.Migrations;

namespace Group3.Semester3.WebApp
{
    public class Program
    {
        /// <summary>
        /// The Main() method of the entire application
        /// </summary>
        /// <param name="args">Not used</param>
        public static void Main(string[] args)
        {
            var serviceProvider = CreateServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates a HostBuilder
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                    });

                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        private static IServiceProvider CreateServices()
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            var connectionString = config.GetConnectionString("DBConnection");

            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQL server support to FluentMigrator
                    .AddSqlServer()
                    // Set the connection string
                    .WithGlobalConnectionString(connectionString)
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(CreateUserTable).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </summary>
        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }
    }
}
