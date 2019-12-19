namespace Jasper.Messaging.Tracking
{
    public enum EventType
    {
        Received,
        Sent,
        ExecutionStarted,
        ExecutionFinished,
        MessageSucceeded,
        MessageFailed,
        NoHandlers,
        NoRoutes,
        MovedToErrorQueue
    }
}
