namespace Jasper.Messaging.Durability
{
    public interface ISchedulingAgent
    {
        void RescheduleOutgoingRecovery();
        void RescheduleIncomingRecovery();
    }
}
