namespace Jasper.Configuration
{
    public static class TransportsExpressionExtensions
    {
        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void LightweightListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}");
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void DurableListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}").IsDurable();
        }
    }
}
