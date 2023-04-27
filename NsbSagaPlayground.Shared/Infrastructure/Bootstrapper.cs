using Microsoft.Data.SqlClient;
using NServiceBus;

namespace NsbSagaPlayground.Shared.Infrastructure;

public class Bootstrapper
{
  public static Task<IEndpointInstance> Start(string endpointName, string connectionString) 
  {
    return Endpoint.Start(Configure(endpointName, connectionString));
  }

  public static EndpointConfiguration Configure(string endpointName, string connectionString)
  {
    var config = new EndpointConfiguration(endpointName);

    ConfigureRouting(config);
    ConfigureTransport(config, connectionString);
    ConfigurePersistence(config, connectionString);

    config.AuditProcessedMessagesTo("audit");
    config.SendFailedMessagesTo("error");
    
    config.EnableInstallers();

    return config;
  }

  #region Internals

  private static void ConfigureRouting(EndpointConfiguration config)
  {
    config.Conventions().DefiningCommandsAs(t => t.Namespace?.Contains("Messages.Commands") ?? false);
    config.Conventions().DefiningEventsAs(t => t.Namespace?.Contains("Messages.Events") ?? false);
  }

  private static void ConfigureTransport(EndpointConfiguration config, string connectionString)
  {
    var transport = config.UseTransport<SqlServerTransport>();
    transport
      .Transactions(TransportTransactionMode.TransactionScope)
      .DefaultSchema("nsb")
      .ConnectionString(connectionString);
  }

  private static void ConfigurePersistence(EndpointConfiguration config, string connectionString)
  {
    var persistence = config.UsePersistence<SqlPersistence>();
    persistence
      .ConnectionBuilder(() => new SqlConnection(connectionString));
    persistence
      .SqlDialect<SqlDialect.MsSqlServer>()
      .Schema("nsb");
  }

  #endregion
}