using Jasper;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Marten;
using Npgsql;

namespace CommandBusSamples;

public record ConfirmReservation(Guid ReservationId);
public record ReservationConfirmed(Guid ReservationId);

public class Reservation
{
    public Guid Id { get; set; }
    public DateTimeOffset Time { get; set; }
    public string RestaurantName { get; set; }
    public bool IsConfirmed { get; set; }
}

public class ConfirmReservationHandler
{
    public async Task Handle(ConfirmReservation command, IDocumentSession session, IExecutionContext publisher)
    {
        // Start the outbox...
        await publisher.EnlistInTransactionAsync(session);

        var reservation = await session.LoadAsync<Reservation>(command.ReservationId);

        reservation!.IsConfirmed = true;

        // Watch out, this could be a race condition!!!!
        await publisher.PublishAsync(new ReservationConfirmed(reservation.Id));

        // We're coming back to this in a bit......
        await session.SaveChangesAsync();
    }
}

// Just assume this service is in the IoC container
public interface IRestaurantProxy
{
    Task NotifyRestaurant(Reservation? reservation);
}

// What about error handling?
[LocalQueue("Notifications")]
[RetryLater(typeof(HttpRequestException), 1, 2, 5)]
public class ReservationConfirmedHandler
{
    public async Task Handle(ReservationConfirmed confirmed, IQuerySession session, IRestaurantProxy restaurant)
    {
        var reservation = await session.LoadAsync<Reservation>(confirmed.ReservationId);

        // Make a call to an external web service through a proxy
        await restaurant.NotifyRestaurant(reservation);
    }
}
