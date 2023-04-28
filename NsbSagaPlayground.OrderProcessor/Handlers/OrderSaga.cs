using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NsbSagaPlayground.Persistence;
using NsbSagaPlayground.Shared.Domain;
using NsbSagaPlayground.Shared.Messages.Commands;
using NsbSagaPlayground.Shared.Messages.Events;
using NServiceBus;

namespace NsbSagaPlayground.OrderProcessor.Handlers;

internal class OrderSaga : Saga<OrderData>,
  IAmStartedByMessages<CreateOrder>,
  IHandleMessages<CancelOrder>,
  IHandleTimeouts<BuyerRemorseExpired>
{
  private readonly AppDbContext _dbContext;
  private readonly ILogger<OrderSaga> _logger;

  public OrderSaga(AppDbContext dbContext, ILogger<OrderSaga> logger)
  {
    _dbContext = dbContext;
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
    _dbContext.Orders.Add(Order.Create(message.Id));
    await _dbContext.SaveChangesAsync();
    
    await RequestTimeout<BuyerRemorseExpired>(context, TimeSpan.FromMinutes(1));
  }

  public async Task Handle(CancelOrder message, IMessageHandlerContext context)
  {
    // if CancelOrder is received after BuyerRemorseExpired, nothing will happens
    // because the saga is complete
    // See https://docs.particular.net/tutorials/nservicebus-sagas/2-timeouts/
    _logger.LogInformation("Processing {Message}", nameof(CancelOrder));

    var order = await _dbContext.Orders.SingleAsync(o => o.UId == message.Id);
    order.Cancel();
    await _dbContext.SaveChangesAsync();
    
    _logger.LogInformation("Order {Id} cancelled", message.Id);
    MarkAsComplete();
  }

  /// <inheritdoc />
  public async Task Timeout(BuyerRemorseExpired state, IMessageHandlerContext context)
  {
    _logger.LogInformation("Grace period to cancel order {Id} has expired: order is confirmed", Data.OrderId);
    
    var order = await _dbContext.Orders.SingleAsync(o => o.UId == Data.OrderId);
    order.Confirm();
    await _dbContext.SaveChangesAsync();
    
    await context.Publish(new OrderConfirmed() {
      Id = Data.OrderId
    });
    
    MarkAsComplete();
  }
}