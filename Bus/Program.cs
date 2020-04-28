namespace BusSeervice
{
    using Contract.Warehouse;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.MongoDbIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Service.Consumers;
    using Service.CourierActivities;
    using Service.StateMachine;
    using Service.Warehouse.Consumers;
    using Service.Warehouse.StateMachines;
    using System;
    using System.Threading.Tasks;


    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<AcceptOrderActivity>();
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocationStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://mongo";
                                r.DatabaseName = "allocations";
                            });

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://mongo";
                                r.DatabaseName = "orderdb";
                            });

                        cfg.AddBus(ConfigureBus);

                        cfg.AddRequestClient<AllocateInventory>();
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

                cfg.UseMessageScheduler(new Uri("queue:quartz"));
                //cfg.UseInMemoryScheduler(); //need this MassTransit.Quartz, for test purposes

                cfg.ConfigureEndpoints(provider);
            });
        }
    }
}