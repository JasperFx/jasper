namespace Jasper.Transports.Tcp
{
    public enum ReceivedStatus
    {
        Successful,
        QueueDoesNotExist,
        SerializationFailure,
        ProcessFailure,
        Acknowledged,
        NotAcknowledged
    }
}
