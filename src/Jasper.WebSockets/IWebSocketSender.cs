namespace Jasper.WebSockets
{
    public interface IWebSocketSender
    {
        void Send(ClientMessage message);
        void Send(params ClientMessage[] messages);
    }
}