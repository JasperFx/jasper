namespace Jasper.Testing.Samples
{
    // SAMPLE: IgnoreValidationErrors
    public class IgnoreValidationErrors : JasperRegistry
    {
        public IgnoreValidationErrors()
        {
            Advanced.ThrowOnValidationErrors = false;
        }
    }

    // ENDSAMPLE
}
