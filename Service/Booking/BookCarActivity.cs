using MassTransit.Courier;
using MassTransit.SignalR.Contracts;
using MassTransit.SignalR.Utils;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading.Tasks;
using WebApi.Hubs;

namespace Service.Booking
{
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

            if(context.Arguments.Message.ToLower() == "bmw")
            {
                throw new System.Exception();
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
