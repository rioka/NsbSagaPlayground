using Microsoft.EntityFrameworkCore;
using NsbSagaPlayground.Persistence;
using NServiceBus;
using NServiceBus.Pipeline;

namespace NsbSagaPlayground.OrderProcessor.Infrastructure.Behaviors;

internal class SetConnectionBehavior : Behavior<IInvokeHandlerContext>
{
  public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
  {
    var dbContext = context.Builder.Build<AppDbContext>();
    var connection = context.SynchronizedStorageSession.SqlPersistenceSession().Connection;
    dbContext.Database.SetDbConnection(connection);
    
    await next();
  }
}