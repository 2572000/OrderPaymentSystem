using Microsoft.EntityFrameworkCore;
using OrderServiceApi.Data;
using OrderServiceApi.Repositories;
using OrderServiceApi.Services;
using Serilog;
using System.Text.Json.Serialization;

namespace OrderServiceApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    });
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddHttpClient<IOrderService, OrderService>(options =>
            {
                options.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"]!);
            });


            #region Serilog Configurations

            //builder.Services.AddSerilog();
            //install package Serilog.AspNetCore && Serilog.Sinks.Seq

            builder.Host.UseSerilog((context,  loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                );


            #endregion


            var app = builder.Build();

            #region Aplay Migrations
            var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();

            #endregion

            app.MapControllers();

            app.UseSerilogRequestLogging();

            app.Run();
        }
    }
}
