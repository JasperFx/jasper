using Baseline.Dates;
using Jasper;

namespace OrderSagaSample;

public record StartOrder(string Id);

public record CompleteOrder(string Id);

public record OrderTimeout(string Id) : TimeoutMessage(1.Minutes());

public class Order : Saga
{
    public string? Id { get; set; }

    public OrderTimeout Start(StartOrder order, ILogger<Order> logger)
    {
        Id = order.Id; // defining the Saga Id.

        logger.LogInformation("Got a new order with id {Id}", order.Id);
        // creating a timeout message for the saga
        return new OrderTimeout(order.Id);
    }

    public void Handle(CompleteOrder complete, ILogger<Order> logger)
    {
        logger.LogInformation("Completing order {Id}", complete.Id);

        // That's it, we're done. Delete the saga state after the message is done.
        MarkCompleted();
    }

    public void Handle(OrderTimeout timeout, ILogger<Order> logger)
    {
        logger.LogInformation("Applying timeout to order {Id}", timeout.Id);

        // That's it, we're done. Delete the saga state after the message is done.
        MarkCompleted();
    }
}
