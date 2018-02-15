using Jasper.Messaging.Runtime.Invocation;

namespace benchmarks
{
    public class PingPongHandler
    {
        public object Handle(Ping ping)
        {
            var pong = new Pong {Id = ping.Id};
            return Respond.With(pong).ToSender();
        }
    }
}