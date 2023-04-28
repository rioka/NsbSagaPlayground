using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NsbSagaPlayground.OrderProcessor.Infrastructure.Behaviors;
using NsbSagaPlayground.Persistence;
using NsbSagaPlayground.Shared;
using NsbSagaPlayground.Shared.Infrastructure;
using NServiceBus;

internal class Program
{
  public static async Task Main(string[] args)
  {
    var host = CreateHostBuilder(args)
      .Build();

    await host.RunAsync();
  }

  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    var hb = Host
      .CreateDefaultBuilder()
      .UseConsoleLifetime()
      .ConfigureServices(s => {
        s.AddScoped<AppDbContext>(sp => {
          
          var configuration = sp.GetRequiredService<IConfiguration>();
          return new AppDbContext(configuration.GetConnectionString("Data"));
        });
      })
      .UseNServiceBus(ctx => {

        var endpointConfig = Bootstrapper.Configure(Endpoints.OrderProcessor, ctx.Configuration.GetConnectionString("Data"));
        endpointConfig.EnableFeature<ForceConnectionFeature>();
        
        // temporary
        endpointConfig.LimitMessageProcessingConcurrencyTo(1);
        
        return endpointConfig;
      });
    
    return hb;
  }
}
