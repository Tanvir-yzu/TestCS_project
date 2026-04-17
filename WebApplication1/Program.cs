using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Note: lightweight API docs endpoint is provided below instead of adding a
            // Swagger package, to avoid requiring an external NuGet restore in this sample.

            // Configure authentication (cookie-based) BEFORE building the app. Calling
            // Build() freezes the service collection; further modifications will throw
            // InvalidOperationException.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = System.TimeSpan.FromHours(8);
                });

            // Register product repository (file-backed JSON) for simple persistence without
            // requiring EF/DB packages. This keeps products across restarts in a local file.
            builder.Services.AddSingleton<WebApplication1.Data.IProductRepository, WebApplication1.Data.FileProductRepository>();
            // Register user repository so registered users persist to users.json
            builder.Services.AddSingleton<WebApplication1.Data.IUserRepository, WebApplication1.Data.FileUserRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // Authentication must be added before Authorization
            app.UseAuthentication();

            // Lightweight docs endpoint for the sample API (no external packages required)
            app.MapGet("/swagger", async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(@"<!doctype html>
<html>
  <head>
    <meta charset='utf-8' />
    <title>AmosWRN API Docs</title>
  </head>
  <body>
    <h1>AmosWRN — Weather API</h1>
    <p>Available endpoints:</p>
    <ul>
      <li><a href='/api/WeatherApi'>GET /api/WeatherApi</a> — Sample 7-day forecast (JSON)</li>
    </ul>
    <p>Use Swagger/Swashbuckle for full OpenAPI support in a real project.</p>
  </body>
</html>");
            });
            // Diagnostic endpoint to confirm the app is serving requests
            app.MapGet("/ping", () => Results.Text("pong"));
            // Redirect legacy /Error path to the Home controller error action
            app.MapGet("/Error", () => Results.Redirect("/Home/Error", permanent: false));
            // Also handle the trailing-slash request which some browsers/servers may add
            app.MapGet("/swagger/", context =>
            {
                context.Response.Redirect("/swagger", permanent: false);
                return System.Threading.Tasks.Task.CompletedTask;
            });

            app.UseAuthorization();

            app.MapStaticAssets();
            // Map attribute-routed API controllers (e.g. [Route("api/[controller]")])
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
