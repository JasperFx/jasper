namespace Jasper.Bus.Transports.Configuration
{
    public interface ITransportExpression : ILoopbackTransportExpression
    {
        void Disable();

        ITransportExpression ListenOnPort(int port);

        ITransportExpression MaximumSendAttempts(int number);
    }
}