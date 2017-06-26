namespace Jasper.Bus
{
    public class BusSettings
    {
        public int ResponsePort { get; set; } = 2333;
        public int MaximumFireAndForgetSendingAttempts { get; set; } = 3;
    }
}
