using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shellty_Blog.Extensions;

namespace Shellty_Blog.Tests;

public class SeoPipelineTests
{
    [Fact]
    public async Task Pipeline_RedirectsAliasPreservesHealthAndAddsNoindexToPrivatePages()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllersWithViews();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        await using var app = builder.Build();
        app.ConfigurePipeline();
        app.MapGet("/Account/SeoAudit", () => Results.Ok());
        app.MapGet("/seo-missing", () => Results.NotFound());
        await app.StartAsync();
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(app.Urls.Single()),
        };

        using var aliasRequest = new HttpRequestMessage(HttpMethod.Get, "/BlogPost/Post/42?source=audit");
        aliasRequest.Headers.Host = "shellty-blog.onrender.com";
        var aliasResponse = await client.SendAsync(aliasRequest);
        Assert.Equal(HttpStatusCode.MovedPermanently, aliasResponse.StatusCode);
        Assert.Equal("https://blog.shellty.pl/BlogPost/Post/42?source=audit",
            aliasResponse.Headers.Location?.ToString());

        using var healthRequest = new HttpRequestMessage(HttpMethod.Get, "/health");
        healthRequest.Headers.Host = "shellty-blog.onrender.com";
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(healthRequest)).StatusCode);

        foreach (var path in new[] { "/Account/SeoAudit", "/seo-missing" })
        {
            var response = await client.GetAsync(path);
            Assert.Equal("noindex, follow", response.Headers.GetValues("X-Robots-Tag").Single());
        }
        await app.StopAsync();
    }
}
