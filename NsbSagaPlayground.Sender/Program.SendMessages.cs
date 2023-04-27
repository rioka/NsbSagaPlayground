using NsbSagaPlayground.Shared;
using NsbSagaPlayground.Shared.Messages.Commands;
using NServiceBus;

internal partial class Program
{
  private static async Task SendMessages(IMessageSession session)
  {
    Console.WriteLine("Press '1' enter to create an order");
    Console.WriteLine("Press '2' enter to cancel most recent order creation request");
    Console.WriteLine("Press any key to exit");

    Guid lastOrder = default;
    var exit = false;
    
    while (!exit)
    {
      var ch = Console.ReadKey();
      Console.WriteLine();

      switch (ch.Key)
      {
        case ConsoleKey.D1:
        case ConsoleKey.NumPad1:
          var createOrder = new CreateOrder() {
            Id = lastOrder = Guid.NewGuid()
          };
          await session.Send(Endpoints.OrderProcessor, createOrder);
          break;

        case ConsoleKey.D2:
        case ConsoleKey.NumPad2:
          if (lastOrder != default)
          {
            var cancelOrder = new CancelOrder() {
              Id = lastOrder
            };
            await session.Send(Endpoints.OrderProcessor, cancelOrder);
            lastOrder = default;
          }
          else
          {
            Console.WriteLine("No order available for cancellation");
          }
          break;
          
        default:
          exit = true;
          break;
      }
    }
  }
}