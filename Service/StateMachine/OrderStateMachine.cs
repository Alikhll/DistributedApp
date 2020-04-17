using Automatonymous;
using Contract.StateMachine;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RedisIntegration;
using System;

namespace Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderStatusRequested, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new OrderNotFound
                        {
                            OrderId = context.Message.OrderId
                        });
                    }
                }));
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.SubmitDate = context.Data.Timestamp;
                    context.Instance.Updated = DateTime.UtcNow;
                })
                .TransitionTo(Submitted));

            During(Submitted, Ignore(OrderSubmitted)); //Idempotnet: don't repeat the message again

            DuringAny(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.SubmitDate = context.Data.Timestamp;
                }));

            DuringAny(
                When(OrderStatusRequested)
                .RespondAsync(x => x.Init<OrderStatus>(new OrderStatus
                {
                    OrderId = x.Instance.CorrelationId,
                    Status = x.Instance.CurrentState
                })));

        }

        public State Submitted { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
    }

    public class OrderState : SagaStateMachineInstance, IVersionedSaga
    {
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }

        public string CurrentState { get; set; }
        public string CustomerNumber { get; set; }
        public DateTime? SubmitDate { get; set; }
        public DateTime? Updated { get; set; }
    }

    public class OrderStateMachineDefinition :
        SagaDefinition<OrderState>
    {
        public OrderStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 10000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
