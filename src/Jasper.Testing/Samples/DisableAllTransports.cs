namespace Jasper.Testing.Samples
{
    // SAMPLE: TransportsAreDisabled
    public class TransportsAreDisabled : JasperRegistry
    {
        public TransportsAreDisabled()
        {
            Settings.AlterMessaging(_ => _.DisableAllTransports = true);
        }
    }
    // ENDSAMPLE


    // SAMPLE: DisableIndividualTransport
    public class DisableOrEnableTransports : JasperRegistry
    {
        public DisableOrEnableTransports()
        {
            // TCP transport is enabled by default
            Transports.DisableTransport("tcp");
        }
    }
    // ENDSAMPLE
}
