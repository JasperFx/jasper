namespace Jasper.Testing.Samples
{
    // SAMPLE: IgnoreValidationErrors
    public class IgnoreValidationErrors : JasperRegistry
    {
        public IgnoreValidationErrors()
        {
            Settings.AlterMessaging(_ => _.ThrowOnValidationErrors = false);
        }
    }
    // ENDSAMPLE
}
