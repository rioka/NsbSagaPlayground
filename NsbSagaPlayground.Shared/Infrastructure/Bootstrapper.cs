using Microsoft.Extensions.Configuration;
using NServiceBus;

namespace NsbSagaPlayground.Shared.Infrastructure;

public class Bootstrapper
{
  public static Task<IEndpointInstance> Start(string endpointName, IConfiguration configuration) 
  {
    return Endpoint.Start(Configure(endpointName, configuration));
  }

  internal static EndpointConfiguration Configure(string endpointName, IConfiguration configuration)
  {
    var config = new EndpointConfiguration(endpointName);

    config.Conventions().DefiningCommandsAs(t => t.Namespace?.Contains("Messages.Commands") ?? false);
    config.Conventions().DefiningEventsAs(t => t.Namespace?.Contains("Messages.Events") ?? false);

    var transport = config.UseTransport<SqlServerTransport>();
    transport
      .Transactions(TransportTransactionMode.TransactionScope)
      .DefaultSchema("nsb")
      .ConnectionString(configuration.GetConnectionString("Data"));
    
    var persistence = config.UsePersistence<SqlPersistence>();
    persistence
      .SqlDialect<SqlDialect.MsSqlServer>()
      .Schema("nsb");

    config.AuditProcessedMessagesTo("audit");
    config.SendFailedMessagesTo("error");
    
    config.EnableInstallers();

    return config;
  }
}