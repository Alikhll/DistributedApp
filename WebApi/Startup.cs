using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Contract.Consumers;
using Contract.StateMachine;

namespace WebApi
{
    public class Startup
    {
        //docker run -d -p 27017:27017 mongo
        //docker run -d -p 5672:5672 -p 8080:15672 rabbitmq:3-management
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(cfg =>
            {
                cfg.AddRequestClient<SubmitOrder>();
                cfg.AddRequestClient<CheckOrder>();

                cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq());
            });
            services.AddMassTransitHostedService();


            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample API Site");
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
