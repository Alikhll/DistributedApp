using Automatonymous;
using Contract.StateMachine;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfilmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));

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
            Event(() => AccountClosed, x => x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber));


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


            //Idempotnet: don't repeat the message again
            During(Submitted,
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .TransitionTo(Canceled),
                When(OrderAccepted)
                    .Activity(x => x.OfType<AcceptOrderActivity>())
                    .TransitionTo(Accepted));

            During(Accepted,
                When(FulfilmentFaulted)
                .TransitionTo(Faulted));

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
                    Status = x.Instance.CurrentState,
                    CustomerNumber = x.Instance.CustomerNumber
                })));

        }

        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Canceled { get; private set; }
        public State Faulted { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
        public Event<CustomerAccountClosed> AccountClosed { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<OrderFulfilmentFaulted> FulfilmentFaulted { get; private set; }


    }

    //IVersionedSaga interface namaspace should be changed upon different db either redis or mongo 
    public class OrderState : SagaStateMachineInstance, IVersionedSaga
    {
        [BsonId]
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
