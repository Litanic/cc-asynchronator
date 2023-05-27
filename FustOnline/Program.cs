using Asynchronator.Logic;
using FustOnline.Logic;

namespace FustOnline
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Inject dependencies.
            builder.Services.AddSingleton<BusEventController>();
            builder.Services.AddSingleton<HttpCommandQueue>();
            builder.Services.AddScoped<HttpCommandProcessor>();

            // Build.
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Run the required services.
            app.Services.GetService<HttpCommandQueue>()?.RemoveFromQueue(Guid.Empty);
            app.Services.GetService<BusEventController>()?.Run();
            app.Run();
        }
    }
}
