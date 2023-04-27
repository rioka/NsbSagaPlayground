using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NsbSagaPlayground.Shared.Infrastructure;
using NServiceBus;

internal partial class Program
{
  public static async Task Main(string[] args)
  {
    var host = CreateHostBuilder(args)
      .Build();

    using (host)
    {
      await host.StartAsync();
      var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
      var session = host.Services.GetRequiredService<IMessageSession>();

      await SendMessages(session); 

      lifetime.StopApplication();
      await host.WaitForShutdownAsync();      
    }
  }

  public static IHostBuilder CreateHostBuilder(string[] args)
  {
    var hb = Host
      .CreateDefaultBuilder()
      .UseConsoleLifetime()
      .UseNServiceBus(ctx => {

        var endpointConfig = Bootstrapper.Configure("Sender", ctx.Configuration.GetConnectionString("Data"));
        return endpointConfig;
      });
    
    return hb;
  }
}
