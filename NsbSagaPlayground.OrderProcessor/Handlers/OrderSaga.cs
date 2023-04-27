using Microsoft.Extensions.Logging;
using NsbSagaPlayground.Shared.Messages.Commands;
using NServiceBus;

namespace NsbSagaPlayground.OrderProcessor.Handlers;

internal class OrderSaga : Saga<OrderData>,
  IAmStartedByMessages<CreateOrder>,
  IHandleMessages<CancelOrder>
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

  public Task Handle(CreateOrder message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {Message}", nameof(CreateOrder));
    return Task.CompletedTask;
  }

  public Task Handle(CancelOrder message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {Message}", nameof(CancelOrder));
    return Task.CompletedTask;
  }
}