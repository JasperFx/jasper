namespace Jasper.Marten.Persistence.Resiliency
{
    public interface ISchedulingAgent
    {
        void RescheduleOutgoingRecovery();
        void RescheduleIncomingRecovery();
    }
}