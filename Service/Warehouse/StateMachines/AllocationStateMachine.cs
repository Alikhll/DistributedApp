using Automatonymous;
using Contract.Warehouse;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Service.Warehouse.StateMachines
{
    public class AllocationStateMachineDefinition : SagaDefinition<AllocationState>
    {
        public AllocationStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<AllocationState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(3, 1000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
    public class AllocationStateMachine : MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));
            Event(() => ReleaseRequested, x => x.CorrelateById(m => m.Message.AllocationId));

            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
            {
                s.Delay = TimeSpan.FromHours(1);
                s.Received = x => x.CorrelateById(m => m.Message.AllocationId);
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(AllocationCreated)
                    .Schedule(HoldExpiration, context => context.Init<AllocationHoldDurationExpired>(new { context.Data.AllocationId }),
                        context => context.Data.HoldDuration)
                    .TransitionTo(Allocated),
                When(ReleaseRequested)
                    .TransitionTo(Released)
            );

            During(Allocated,
                When(AllocationCreated)
                    .Schedule(HoldExpiration, context => context.Init<AllocationHoldDurationExpired>(new { context.Data.AllocationId }),
                        context => context.Data.HoldDuration)
            );

            During(Released,
                When(AllocationCreated)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation already released: {context.Instance.CorrelationId}"))
                    .Finalize()
            );

            During(Allocated,
                When(HoldExpiration.Received)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation expired {context.Instance.CorrelationId}"))
                    .Finalize(),
                When(ReleaseRequested)
                    .Unschedule(HoldExpiration)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation release request, granted {context.Instance.CorrelationId}"))
                    .Finalize()
                 );

            SetCompletedWhenFinalized();
        }

        public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; set; }

        public State Allocated { get; set; }
        public State Released { get; set; }


        public Event<AllocationCreated> AllocationCreated { get; set; }
        public Event<AllocationReleaseRequested> ReleaseRequested { get; set; }
    }

    public class AllocationState :
        SagaStateMachineInstance,
        IVersionedSaga
    {
        [BsonId]
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public Guid? HoldDurationToken { get; set; }

        public int Version { get; set; }
    }
}
