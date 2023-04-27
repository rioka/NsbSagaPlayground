using NServiceBus;

namespace NsbSagaPlayground.OrderProcessor.Handlers;

internal class OrderData : ContainSagaData
{
  public Guid OrderId { get; set; }
}