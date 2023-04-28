namespace NsbSagaPlayground.Shared.Domain;

public class Order
{
  public int Id { get; private set; }

  public Guid UId { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime? ConfirmedAt { get; private set; }
  
  public DateTime? CancelledAt { get; private set; }
  
  private Order()
  { }

  public static Order Create(Guid uid)
  {
    return new Order() {
      UId = uid,
      CreatedAt = DateTime.UtcNow
    };
  }

  public void Confirm()
  {
    if (CancelledAt.HasValue)
    {
      throw new InvalidOperationException($"Order {UId} has been cancelled");
    }

    ConfirmedAt ??= DateTime.UtcNow;
  }
  
  public void Cancel()
  {
    if (ConfirmedAt.HasValue)
    {
      throw new InvalidOperationException($"Order {UId} has been confirmed");
    }
    CancelledAt ??= DateTime.UtcNow;
  }
}