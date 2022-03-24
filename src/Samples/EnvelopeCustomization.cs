using Baseline.Dates;
using Jasper;
using Jasper.Tcp;

namespace Samples
{
    #region sample_MonitoringDataPublisher
    public class MonitoringDataPublisher : JasperOptions
    {
        public MonitoringDataPublisher()
        {
            PublishAllMessages()
                .ToPort(2222)

                // Set a message expiration on all
                // outgoing messages to this
                // endpoint
                .CustomizeOutgoing(env => env.DeliverWithin(2.Seconds()));
        }
    }
    #endregion
}
