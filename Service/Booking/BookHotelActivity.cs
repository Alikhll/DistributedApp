using MassTransit.Courier;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Booking
{
    public class BookHotelActivity : IActivity<BookHotelArgument, BookHotelLogs>
    {
        public async Task<CompensationResult> Compensate(CompensateContext<BookHotelLogs> context)
        {
            await Task.Delay(BookItem.LongDelay);

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Hotel {context.Log.Message} was canceled!" })
            });

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<BookHotelArgument> context)
        {
            await Task.Delay(BookItem.LongDelay);

            var protocols = new IHubProtocol[] { new JsonHubProtocol() };
            await context.Publish<All<ChatHub>>(new
            {
                Messages = protocols.ToProtocolDictionary("ReceiveMessage", new object[] { "message", $"Hotel {context.Arguments.Message} was booked!" })
            });

            return context.Completed(new { Message = context.Arguments.Message });
        }
    }

    public class BookHotelArgument
    {
        public string Message { get; set; }
    }

    public class BookHotelLogs
    {
        public string Message { get; set; }
    }
}
