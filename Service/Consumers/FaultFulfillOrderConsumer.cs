using Contract.Consumers;
using MassTransit;
using System.Threading.Tasks;

namespace Service.Consumers
{
    public class FaultFulfillOrderConsumer : IConsumer<Fault<FulfillOrder>>
    {
        public Task Consume(ConsumeContext<Fault<FulfillOrder>> context)
        {
            System.Console.WriteLine("Add some error logging herer");
            return Task.FromResult(0);
        }
    }
}
