namespace Jasper.Configuration
{
    public interface IFullTransportsExpression : ITransportsExpression
    {
        /// <summary>
        /// Directs Jasper to set up an incoming message listener for the Uri
        /// specified by IConfiguration[configKey]
        /// </summary>
        /// <param name="configKey">The name of an expected configuration item that holds the designated listener Uri</param>
        void ListenForMessagesFromUriValueInConfig(string configKey);
    }
}