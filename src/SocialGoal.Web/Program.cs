using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SocialGoal.Web.Data;

namespace SocialGoal.Web;

public class Program
{
    public static void Main(string[] args)
    {
        // Bootstrap logger: catches host-startup failures before configuration
        // is loaded; replaced by the configured logger in UseSerilog below.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

            // Global antiforgery from day one (security review S5 LOW-1):
            // free while the host is GET-only, and the Phase 2 standing
            // constraint becomes the pipeline default rather than something
            // each mutating slice must remember.
            builder.Services.AddControllersWithViews(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            builder.Services.AddHealthChecks();
            builder.Services.AddSingleton(TimeProvider.System);

            // Absent connection string leaves the host serving DB-free routes
            // (health, error pages); data-backed controllers then fail loudly
            // at resolution rather than silently pointing somewhere implicit.
            var connectionString = builder.Configuration.GetConnectionString("SocialGoal");
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Services.AddDbContext<SocialGoalDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Key ring outside the content root so keys survive deploys; the
            // Azure Blob store replaces the file path at D2 deploy time.
            var keyPath = builder.Configuration["DataProtection:KeyPath"];
            if (!string.IsNullOrEmpty(keyPath))
            {
                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapHealthChecks("/health");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
