using Jasper;

namespace Samples
{
    #region sample_IgnoreValidationErrors
    public class IgnoreValidationErrors : JasperOptions
    {
        public IgnoreValidationErrors()
        {
            Advanced.ThrowOnValidationErrors = false;
        }
    }

    #endregion
}
