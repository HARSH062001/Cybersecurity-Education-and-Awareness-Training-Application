using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CybersecurityTrainingWeb.Tests;

public sealed class AssessmentTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"cybertraining-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        $"Data Source={_databasePath}"
                });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the app's database configuration for this test.
            services.RemoveAll<
                IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlite($"Data Source={_databasePath}")
                    .UseSeeding((context, _) => QuestionBankSeeder.Seed(
                        context,
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "Data",
                            "question_banks.json"))));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme =
                    TestAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions,
                TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                _ => { });
        });
    }

    public void PrepareDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var actualPath = Path.GetFullPath(
            db.Database.GetDbConnection().DataSource);

        if (!string.Equals(
            actualPath,
            Path.GetFullPath(_databasePath),
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The test database path was not applied. " +
                "Refusing to modify another database.");
        }

        db.Database.EnsureCreated();

        db.Users.AddRange(
            new ApplicationUser
            {
                Id = "test-learner-a",
                UserName = "learner-a@example.test",
                NormalizedUserName = "LEARNER-A@EXAMPLE.TEST",
                Email = "learner-a@example.test",
                NormalizedEmail = "LEARNER-A@EXAMPLE.TEST",
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new ApplicationUser
            {
                Id = "test-learner-b",
                UserName = "learner-b@example.test",
                NormalizedUserName = "LEARNER-B@EXAMPLE.TEST",
                Email = "learner-b@example.test",
                NormalizedEmail = "LEARNER-B@EXAMPLE.TEST",
                SecurityStamp = Guid.NewGuid().ToString()
            });

        db.SaveChanges();
    }
}