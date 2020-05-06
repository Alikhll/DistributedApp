using GreenPipes;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Definition;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Booking
{
    public class BookCarActivityDefinition :
        ActivityDefinition<BookCarActivity, BookCarArgument, BookCarLogs>
    {
        public BookCarActivityDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureExecuteActivity(IReceiveEndpointConfigurator endpointConfigurator, IExecuteActivityConfigurator<BookCarActivity, BookCarArgument> executeActivityConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r =>
            {
                r.Interval(2, 1000);
            });
        }
    }

    public class BookCarActivity : IActivity<BookCarArgument, BookCarLogs>
    {
        public async Task<CompensationResult> Compensate(CompensateContext<BookCarLogs> context)
        {
            await Task.Delay(BookItem.LongDelay);

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Car {context.Log.Message} was canceled!" })
            });

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<BookCarArgument> context)
        {
            await Task.Delay(BookItem.LongDelay);

            if (context.Arguments.Message.ToLower() == "bmw")
            {
                throw new System.InvalidOperationException();
            }

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Car {context.Arguments.Message} was booked!" })
            });

            return context.Completed(new { Message = context.Arguments.Message });
        }
    }

    public class BookCarArgument
    {
        public string Message { get; set; }
    }

    public class BookCarLogs
    {
        public string Message { get; set; }
    }
}
