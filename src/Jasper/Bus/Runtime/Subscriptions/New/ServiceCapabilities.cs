namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class ServiceCapabilities
    {
        public string ServiceName { get; set; }
        public PublishedMessage[] Published { get; set; }
        public NewSubscription[] Subscriptions { get; set; }

        public string[] Errors { get; set; } = new string[0];
    }
}
