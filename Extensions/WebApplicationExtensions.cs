using Microsoft.EntityFrameworkCore;
using Shellty_Blog.Data;

namespace Shellty_Blog.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
        db.Database.Migrate();

        return app;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            // Redirect only the production alias; health checks and form posts
            // retain their existing behavior.
            if (context.Request.Host.Host.Equals("shellty-blog.onrender.com", StringComparison.OrdinalIgnoreCase)
                && (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
                && !context.Request.Path.Equals("/health"))
            {
                context.Response.Redirect(
                    $"https://blog.shellty.pl{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}",
                    permanent: true);
                return;
            }

            context.Response.OnStarting(() =>
            {
                if (context.Request.Path.StartsWithSegments("/Account")
                    || context.Request.Path.StartsWithSegments("/Admin")
                    || context.Response.StatusCode >= 400)
                    context.Response.Headers["X-Robots-Tag"] = "noindex, follow";
                return Task.CompletedTask;
            });
            await next(context);
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok("healthy"));

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
