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

public static class ConfirmReservationHandler
{
    [Transactional]
    public static async Task<ReservationConfirmed> Handle(ConfirmReservation command, IDocumentSession session)
    {
        var reservation = await session.LoadAsync<Reservation>(command.ReservationId);

        reservation!.IsConfirmed = true;

        session.Store(reservation);

        // Kicking out a "cascaded" message
        return new ReservationConfirmed(reservation.Id);
    }
}

// Just assume this service is in the IoC container
public interface IRestaurantProxy
{
    Task NotifyRestaurant(Reservation? reservation);
}

// What about error handling?
[LocalQueue("Notifications")]
[RetryNow(typeof(HttpRequestException), 50, 100, 250)]
public class ReservationConfirmedHandler
{
    public async Task Handle(ReservationConfirmed confirmed, IQuerySession session, IRestaurantProxy restaurant)
    {
        var reservation = await session.LoadAsync<Reservation>(confirmed.ReservationId);

        // Make a call to an external web service through a proxy
        await restaurant.NotifyRestaurant(reservation);
    }
}
