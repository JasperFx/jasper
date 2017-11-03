using System;

namespace Jasper.Bus.Transports.Configuration
{
    public interface ITransportsExpression
    {
        void ListenForMessagesFrom(Uri uri);
        void ListenForMessagesFrom(string uriString);
        void DefaultIs(string uriString);
        void DefaultIs(Uri uri);
        void ExecuteAllMessagesLocally();
    }

    public static class TransportsExpressionExtensions
    {
        public static void LightweightListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}");
        }

        public static void DurableListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}/durable");
        }
    }
}
