using Jasper;

namespace Samples
{
    #region sample_DurableScheduledMessagesLocalQueue
    public class ConfigureDurableLocalQueueApp : JasperOptions
    {
        public ConfigureDurableLocalQueueApp()
        {
            DurableScheduledMessagesLocalQueue

                // Allow no more than 3 scheduled messages
                // to execute at one time
                .MaximumThreads(3);
        }
    }
    #endregion
}
