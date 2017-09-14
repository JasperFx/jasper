namespace Jasper.Testing.Samples
{
    // SAMPLE: TransportsAreDisabled
    public class TransportsAreDisabled : JasperRegistry
    {
        public TransportsAreDisabled()
        {
            Advanced.DisableAllTransports = true;
        }
    }
    // ENDSAMPLE
}
