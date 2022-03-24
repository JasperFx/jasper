using Baseline.Dates;

namespace Jasper.Persistence.Testing.Samples
{
    #region sample_AdvancedConfigurationOfDurabilityAgent
    public class DurabilityAgentCustomization : JasperOptions
    {
        public DurabilityAgentCustomization()
        {
            // Control the maximum batch size of recovered
            // messages that the current node will try
            // to pull into itself
            Advanced.RecoveryBatchSize = 500;


            // How soon should the first node reassignment
            // execution to try to look for dormant nodes
            // run?
            Advanced.FirstNodeReassignmentExecution = 1.Seconds();

            // Fine tune how the polling for ready to execute
            // or send scheduled messages
            Advanced.ScheduledJobFirstExecution = 0.Seconds();
            Advanced.ScheduledJobPollingTime = 60.Seconds();
        }
    }
    #endregion
}
