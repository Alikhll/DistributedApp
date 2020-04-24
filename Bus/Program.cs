﻿namespace BusSeervice
{
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.MongoDbIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Service.Consumers;
    using Service.StateMachine;
    using System;
    using System.Threading.Tasks;


    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://mongo";
                                r.DatabaseName = "orderdb";
                            });

                        cfg.AddBus(ConfigureBus);
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("rabbitmq://rabbitmq");
                cfg.ConfigureEndpoints(provider);
            });
        }
    }
}