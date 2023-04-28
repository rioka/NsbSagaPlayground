using NServiceBus.Features;

namespace NsbSagaPlayground.OrderProcessor.Infrastructure.Behaviors;

internal class ForceConnectionFeature : Feature
{
  /// <inheritdoc />
  protected override void Setup(FeatureConfigurationContext context)
  {
    context.Pipeline.Register(new SetConnectionBehavior(), "DbContext uses same connection as persistence");
  }
}