using CarvedRock.Data.Entities;
using CarvedRock.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using RestEase;
using WireMock.Client;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Bogus;
using CarvedRock.Core;

namespace CarvedRock.InnerLoop.Tests.Utilities;

public class SharedFixture : IAsyncLifetime
{
    public readonly Faker<NewProductModel> NewProductFaker = new Faker<NewProductModel>()
        .UseSeed(2001)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Category, f => f.PickRandom("boots", "equip", "kayak"))
        .RuleFor(p => p.Price, (f, p) =>
                p.Category == "boots" ? f.Random.Double(50, 300) :
                p.Category == "equip" ? f.Random.Double(20, 150) :
                p.Category == "kayak" ? f.Random.Double(100, 500) : 0)
        .RuleFor(p => p.ImgUrl, f => f.Image.PicsumUrl());

    // see sqlite docs for more options
    public const string DatabaseName = "InMemTestDb;Mode=Memory;Cache=Shared;";
    public string PostgresConnectionString => _dbContainer?.GetConnectionString() ?? string.Empty;
    public string SqlConnectionString => _sqlContainer?.GetConnectionString() ?? string.Empty;

    private string? _mockServerUrl;

    public List<Product>? OriginalProducts { get; private set; }

    private LocalContext? _dbContext;
    private PostgreSqlContainer? _dbContainer;
    private MsSqlContainer? _sqlContainer;

    // Custom SQL Server -------------------------------------------------------    
    private IContainer? _customSqlContainer;

    public string CustomSqlConnectionString =>
        _customSqlContainer != null
        ? $"Server=127.0.0.1,{_customSqlContainer.GetMappedPublicPort(1433)};" +
          "Database=carvedrock;" +
          "User=sa;" +
          "Password=Custom1zationRocks!;" +
          "MultipleActiveResultSets=true;" +
          "TrustServerCertificate=true;"
        : string.Empty;
    // ----------------------------------------------------

    public async Task InitializeAsync()
    {
        // Try to initialize Testcontainers only if Docker is available.
        bool dockerAvailable = IsDockerAvailable();
        if (dockerAvailable)
        {
            try
            {
                // Use explicit image constructors to avoid obsolete parameterless constructors
                _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
                    .WithDatabase("carvedrock")
                    .WithUsername("carvedrock")
                    .WithPassword("innerloop-ftw!")
                    .Build();

                _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                    .WithPassword("1nnerLoop-ftw!")
                    .Build();

                _customSqlContainer = new ContainerBuilder("localhost/carvedrock/sqlserver")
                    .WithEnvironment("SA_PASSWORD", "Custom1zationRocks!")
                    .WithPortBinding(1433, assignRandomHostPort: true)
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Starting up database 'CarvedRock'."))
                    .WithCleanUp(true)
                    .Build();

                // Start containers if built
                await _dbContainer.StartAsync();
                await _sqlContainer.StartAsync();
                await _customSqlContainer.StartAsync();
            }
            catch
            {
                // If any container initialization fails, fall back to SQLite in-memory
                dockerAvailable = false;
            }
        }

        // SQLite fallback (used when Docker/Testcontainers are not available)
        var options = new DbContextOptionsBuilder<LocalContext>()
            .UseSqlite($"Data Source={DatabaseName}")
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        _dbContext = new LocalContext(options);

        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
        await _dbContext.Database.OpenConnectionAsync();

        // SQL Server ---------------------------
        //await _sqlContainer.StartAsync(); // built-in testcontainer sql
        //var optionsBuilder = new DbContextOptionsBuilder<LocalContext>()
        //    .UseSqlServer(SqlConnectionString);

        //await _customSqlContainer.StartAsync(); // custom sql
        //var optionsBuilder = new DbContextOptionsBuilder<LocalContext>()
        //    .UseSqlServer(CustomSqlConnectionString);

        //_dbContext = new LocalContext(optionsBuilder.Options);
        //---------------------------------------

        await _dbContext.Database.MigrateAsync();
        _dbContext.InitializeTestData(50);

        OriginalProducts = await _dbContext.Products.ToListAsync();
    }

    public string ProxyAndRecordApiCalls(Uri apiBaseUrl)
    {
        var server = WireMockServer.StartWithAdminInterface();
        _mockServerUrl = server.Url!;

        server.Given(Request.Create().WithPath("*"))
            .RespondWith(Response.Create().WithProxy(
                new ProxyAndRecordSettings
                {
                    Url = apiBaseUrl.ToString(),
                    SaveMapping = true,
                    SaveMappingToFile = true
                }));

        return _mockServerUrl;
    }

    public async Task DisposeAsync()
    {
        if (!string.IsNullOrEmpty(_mockServerUrl))
        {
            var api = RestClient.For<IWireMockAdminApi>(_mockServerUrl);
            var getMappingsResult = await api.GetMappingsAsync();
        }

        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        // Stop and dispose containers if they were started
        if (_dbContainer != null)
        {
            try { await _dbContainer.StopAsync(); } catch { }
            _dbContainer = null;
        }

        if (_sqlContainer != null)
        {
            try { await _sqlContainer.StopAsync(); } catch { }
            _sqlContainer = null;
        }

        if (_customSqlContainer != null)
        {
            try { await _customSqlContainer.StopAsync(); } catch { }
            _customSqlContainer = null;
        }
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return false;
            if (!p.WaitForExit(3000))
            {
                try { p.Kill(); } catch { }
                return false;
            }
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

[CollectionDefinition(nameof(InnerLoopCollection))]
public class InnerLoopCollection : ICollectionFixture<SharedFixture>
{
    // This class has no code, and is never created. Its purpose is simply to be the place
    // to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}