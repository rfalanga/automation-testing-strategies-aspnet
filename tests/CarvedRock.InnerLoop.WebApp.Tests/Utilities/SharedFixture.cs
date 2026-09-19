using Bogus;
using CarvedRock.Core;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarvedRock.InnerLoop.WebApp.Tests.Utilities;

[CollectionDefinition(nameof(InnerLoopCollection))]
public class InnerLoopCollection : ICollectionFixture<SharedFixture>
{
    // This class has no code, and is never created. Its purpose is simply to be the place
    // to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}

public class SharedFixture : IAsyncLifetime
{
    public readonly Faker Faker = new();
    public List<ProductModel> OriginalProducts { get; private set; } = null!;
    public List<EmailModel> SentEmails { get; } = new();

    private static readonly List<string> _categories = new() { "boots", "equip", "kayak" };

    public readonly Faker<ProductModel> ProductFaker = new Faker<ProductModel>()
        .RuleFor(p => p.Id, f => f.UniqueIndex + 1)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Price, f => f.Random.Double(10, 1000))
        .RuleFor(p => p.Category, f => f.PickRandom(_categories))
        .RuleFor(p => p.ImgUrl, f => f.Image.PicsumUrl());    

    public async Task InitializeAsync()
    {
        bool dockerAvailable = IsDockerAvailable();
        if (dockerAvailable)
        {
            try
            {
                // Build smtp4dev container using explicit image-based constructor to avoid obsolete APIs
                _emailContainer = new ContainerBuilder("rnwood/smtp4dev/smtp4dev:latest")
                    .WithPortBinding(80, assignRandomHostPort: true)
                    .WithPortBinding(25, assignRandomHostPort: true)
                    .WithCleanUp(true)
                    .Build();

                await _emailContainer.StartAsync();
            }
            catch
            {
                // fall back to in-process mocks if container build/start fails
                dockerAvailable = false;
                _emailContainer = null;
            }
        }

        OriginalProducts = ProductFaker.Generate(10);

        ProductServiceUrl = StartWireMockForProductService();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    // SMTP4DEV Email Server ---------------------------
    public string EmailServerUrl => _emailContainer != null ? $"http://localhost:{_emailContainer.GetMappedPublicPort(80)}" : string.Empty;
    public ushort EmailPort => _emailContainer != null ? (ushort)_emailContainer.GetMappedPublicPort(25) : (ushort)0;

    private IContainer? _emailContainer;

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

    //// WireMock for Product Service --------------------
    public string ProductServiceUrl { get; private set; } = null!;

    private string StartWireMockForProductService()
    {
        var server = WireMockServer.Start();

        // NOTES: Order matters -- make the MORE SPECIFIC routes AFTER the more general ones
        // Also - the "WithPath" method needs to have the forward /
        server
            .Given(Request.Create().WithPath("/Product").WithParam("category").UsingGet())
            .RespondWith(Response.Create().WithBodyAsJson(GetProductsBasedOnCategory));

        server
            .Given(Request.Create().WithPath("/Product").WithParam("category", "error").UsingGet())
            .RespondWith(Response.Create()
            .WithDelay(TimeSpan.FromMilliseconds(Faker.Random.Int(0, 4000))) // delay anywhere from 0 to 4 seconds
            .WithBodyAsJson(GetProblemDetail)
            .WithStatusCode(500));

        return server.Urls[0];
    }

    private object GetProblemDetail(IRequestMessage message)
    {
        return new ProblemDetails
        {
            Detail = "An error occurred while processing your request.",
            Status = 500,
            Title = "Internal Server Error",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }

    private object GetProductsBasedOnCategory(IRequestMessage message)
    {
        var category = message.GetParameter("category")!.First();
        return category == "all"
            ? OriginalProducts
            : OriginalProducts.Where(p => p.Category == category).ToList();
    }
}
