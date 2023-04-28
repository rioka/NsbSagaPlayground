using Microsoft.Extensions.Logging;
using NsbSagaPlayground.Shared.Messages.Commands;
using NsbSagaPlayground.Shared.Messages.Events;
using NServiceBus;

namespace NsbSagaPlayground.OrderProcessor.Handlers;

internal class OrderSaga : Saga<OrderData>,
  IAmStartedByMessages<CreateOrder>,
  IHandleMessages<CancelOrder>,
  IHandleTimeouts<BuyerRemorseExpired>
{
  private readonly ILogger<OrderSaga> _logger;

  public OrderSaga(ILogger<OrderSaga> logger)
  {
    _logger = logger;
  }
  
  protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderData> mapper)
  {
    mapper.MapSaga(saga => saga.OrderId)
      .ToMessage<CreateOrder>(m => m.Id)
      .ToMessage<CancelOrder>(m => m.Id);
  }

  public async Task Handle(CreateOrder message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {Message}", nameof(CreateOrder));

    // TODO insert order
    await RequestTimeout<BuyerRemorseExpired>(context, TimeSpan.FromMinutes(1));
  }

  public Task Handle(CancelOrder message, IMessageHandlerContext context)
  {
    // if CancelOrder is received after BuyerRemorseExpired, nothing will happens
    // because the saga is complete
    // See https://docs.particular.net/tutorials/nservicebus-sagas/2-timeouts/
    _logger.LogInformation("Processing {Message}", nameof(CancelOrder));

    _logger.LogInformation("Order {Id} cancelled", message.Id);
    MarkAsComplete();
    
    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public async Task Timeout(BuyerRemorseExpired state, IMessageHandlerContext context)
  {
    _logger.LogInformation("Grace period to cancel order {Id} has expired: order is confirmed", Data.OrderId);
    
    // TODO update order (ConfirmedAt)
    await context.Publish(new OrderConfirmed() {
      Id = Data.OrderId
    });
    
    MarkAsComplete();
  }
}