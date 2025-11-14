using ABC_Retail_Part1.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ABC_Retail_Part1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC support
            builder.Services.AddControllersWithViews();

            // Get Azure Storage connection string
            var connectionString = builder.Configuration["AzureStorage:ConnectionString"]
                               ?? throw new InvalidOperationException("AzureStorage:ConnectionString not found.");

            // Get deployed Functions URL from config
            var functionsBaseUrl = builder.Configuration["FunctionsApi:BaseUrl"]
                                 ?? throw new InvalidOperationException("FunctionsApi:BaseUrl not found.");

            // Register services
            builder.Services.AddSingleton(new TableService(connectionString));
            builder.Services.AddSingleton(new BlobService(connectionString));
            builder.Services.AddSingleton(new QueueService(connectionString));
            builder.Services.AddSingleton(new FileService(connectionString, functionsBaseUrl));

            // Register session and IHttpContextAccessor
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(8); // not forcing login each page
            });
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
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
