using Automatonymous;
using Contract.Booking;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Service.Booking
{
    public class BookingStateMachineDefinition : SagaDefinition<BookingState>
    {
        public BookingStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<BookingState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 10000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

    public class BookingStateMachine : MassTransitStateMachine<BookingState>
    {
        public BookingStateMachine()
        {
            Event(() => BookingAccepted, x => x.CorrelateById(m => m.Message.BookingId));
            Event(() => BookingFinalized, x => x.CorrelateById(m => m.Message.BookingId));
            Event(() => BookingFaulted, x => x.CorrelateById(m => m.Message.BookingId));

            InstanceState(x => x.CurrentState);

            Initially(
                When(BookingAccepted)
                    .Activity(x => x.OfType<BookingAcceptedActivity>())
                    .TransitionTo(Accepted));

            During(Accepted,
                When(BookingFinalized)
                    .Activity(x => x.OfType<BookingFinalizedActivity>())
                    .TransitionTo(Finalized),
                When(BookingFaulted)
                    .TransitionTo(Faulted));
        }

        public State Accepted { get; private set; }
        public State Finalized { get; private set; }
        public State Faulted { get; private set; }

        public Event<BookingAccepted> BookingAccepted { get; private set; }
        public Event<BookingFinalized> BookingFinalized { get; private set; }
        public Event<BookingFaulted> BookingFaulted { get; private set; }
    }

    //IVersionedSaga interface namaspace should be changed upon different db either redis or mongo 
    public class BookingState : SagaStateMachineInstance, IVersionedSaga
    {
        [BsonId]
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }

        public string CurrentState { get; set; }
    }
}
