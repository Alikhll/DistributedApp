using Automatonymous;
using Contract.Warehouse;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Service.Warehouse.StateMachines
{
    public class AllocationStateMachine : MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));

            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
            {
                s.Delay = TimeSpan.FromSeconds(5);
                s.Received = x => x.CorrelateById(m => m.Message.AllocationId);
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(AllocationCreated)
                .Schedule(HoldExpiration, context => context.Init<AllocationHoldDurationExpired>(new AllocationHoldDurationExpired
                {
                    AllocationId = context.Data.AllocationId
                }), context => context.Data.HoldDuration)
                .TransitionTo(Allocated));

            During(Allocated,
                When(HoldExpiration.Received)
                .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation was released {context.Instance.CorrelationId}"))
                .TransitionTo(Released));

            SetCompletedWhenFinalized();
        }

        public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; set; }

        public State Allocated { get; set; }
        public State Released { get; set; }


        public Event<AllocationCreated> AllocationCreated { get; set; }
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
