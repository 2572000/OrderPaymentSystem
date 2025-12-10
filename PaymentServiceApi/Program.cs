using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace PaymentServiceApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });



            var app = builder.Build();


            app.MapControllers();

            app.MapGet("/", () => "Hello World!");



            app.Run();
        }
    }
}
