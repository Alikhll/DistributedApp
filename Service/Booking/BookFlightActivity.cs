using MassTransit.Courier;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Booking
{
    public class BookFlightActivity : IActivity<BookFlightArgument, BookFlightLogs>
    {
        public async Task<CompensationResult> Compensate(CompensateContext<BookFlightLogs> context)
        {
            await Task.Delay(BookItem.LongDelay);

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Flight {context.Log.Message} was canceled!" })
            });

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<BookFlightArgument> context)
        {
            await Task.Delay(BookItem.LongDelay);

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Flight {context.Arguments.Message} was booked!" })
            });

            return context.Completed(new { Message = context.Arguments.Message });
        }
    }

    public class BookFlightArgument
    {
        public string Message { get; set; }
    }

    public class BookFlightLogs
    {
        public string Message { get; set; }
    }
}
