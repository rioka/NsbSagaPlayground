using Microsoft.Extensions.Logging;
using NsbSagaPlayground.Shared.Messages.Events;
using NServiceBus;

namespace NsbSagaPlayground.Sender.Handlers;

internal class OrderCancelledHandler : IHandleMessages<OrderCancelled>
{
  private readonly ILogger<OrderCancelledHandler> _logger;

  public OrderCancelledHandler(ILogger<OrderCancelledHandler> logger)
  {
    _logger = logger;
  }
  
  public Task Handle(OrderCancelled message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Order {Id} has been cancelled", message.Id);
    return Task.CompletedTask;
  }
}