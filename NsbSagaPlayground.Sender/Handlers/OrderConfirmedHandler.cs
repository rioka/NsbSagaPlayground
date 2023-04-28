using Microsoft.Extensions.Logging;
using NsbSagaPlayground.Shared.Messages.Events;
using NServiceBus;

namespace NsbSagaPlayground.Sender.Handlers;

public class OrderConfirmedHandler : IHandleMessages<OrderConfirmed>
{
  private readonly ILogger<OrderConfirmedHandler> _logger;

  public OrderConfirmedHandler(ILogger<OrderConfirmedHandler> logger)
  {
    _logger = logger;
  }
  /// <inheritdoc />
  public Task Handle(OrderConfirmed message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Order {Id} has been cancelled", message.Id);
    return Task.CompletedTask;
  }
}