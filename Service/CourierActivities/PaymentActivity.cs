using MassTransit.Courier;
using System;
using System.Threading.Tasks;

namespace Service.CourierActivities
{
    public class PaymentActivity : IActivity<PaymentArgument, PaymentLogs>
    {
        static readonly Random _random = new Random();

        public async Task<CompensationResult> Compensate(CompensateContext<PaymentLogs> context)
        {
            await Task.Delay(100);

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<PaymentArgument> context)
        {
            string cardNumber = context.Arguments.CardNumber;
            if (string.IsNullOrEmpty(cardNumber))
                throw new ArgumentNullException(nameof(cardNumber));

            await Task.Delay(1000);
            //await Task.Delay(_random.Next(10000));

            if (cardNumber.StartsWith("5999"))
            {
                throw new InvalidOperationException("The card number was invalid");
            }

            return context.Completed(new { AuthorizationCode = "77777" });
        }
    }

    public class PaymentArgument
    {
        public Guid OrderId { get; set; }
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentLogs
    {
        public string AuthorizationCode { get; set; }
    }
}
