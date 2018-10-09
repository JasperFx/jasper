namespace Jasper.Testing.Samples
{
    // SAMPLE: IgnoreValidationErrors
    public class IgnoreValidationErrors : JasperRegistry
    {
        public IgnoreValidationErrors()
        {
            Settings.Messaging(_ => _.ThrowOnValidationErrors = false);
        }
    }
    // ENDSAMPLE
}
