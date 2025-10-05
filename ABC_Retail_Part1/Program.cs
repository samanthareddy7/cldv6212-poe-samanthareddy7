using ABC_Retail_Part1.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ABC_Retail_Part1
{
    public class Program
    //final
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC support
            builder.Services.AddControllersWithViews();

            // Register Azure storage services (use dependency injection)
            var connectionString = builder.Configuration["AzureStorage:ConnectionString"]
                        ?? throw new InvalidOperationException("AzureStorage:ConnectionString not found.");

            builder.Services.AddSingleton(new TableService(connectionString));
            builder.Services.AddSingleton(new BlobService(connectionString));
            builder.Services.AddSingleton(new QueueService(connectionString));
            builder.Services.AddSingleton(new FileService(connectionString));

            var app = builder.Build();

            // Error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
