using Dapper;
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

    var level = await GetIsolationLevel();
    _logger.LogInformation("Isolation level is {IsolationLevel}", level);
    
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

    var level = await GetIsolationLevel();
    _logger.LogInformation("Isolation level is {IsolationLevel}", level);

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

    var level = await GetIsolationLevel();
    _logger.LogInformation("Isolation level is {IsolationLevel}", level);
    
    var order = await _dbContext.Orders.SingleAsync(o => o.UId == Data.OrderId);
    order.Confirm();
    await _dbContext.SaveChangesAsync();
    
    await context.Publish(new OrderConfirmed() {
      Id = Data.OrderId
    });
    
    MarkAsComplete();
  }
  
  private async Task<string> GetIsolationLevel()
  {
    return await _dbContext
      .Database
      .GetDbConnection()
      .ExecuteScalarAsync<string>(@"
SELECT 
  CASE transaction_isolation_level
    WHEN 1 THEN 'ReadUncomitted'
    WHEN 2 THEN 'ReadCommitted'
    WHEN 3 THEN 'Repeatable'
    WHEN 4 THEN 'Serializable'
    WHEN 5 THEN 'Snapshot'
    ELSE 'Unspecified' 
  END AS transaction_isolation_level,
  sh.text, ph.query_plan
FROM 
  sys.dm_exec_requests
  CROSS APPLY sys.dm_exec_sql_text(sql_handle) sh
  CROSS APPLY sys.dm_exec_query_plan(plan_handle) ph
");
  }
}