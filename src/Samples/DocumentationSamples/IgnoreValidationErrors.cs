using Jasper;
using Microsoft.Extensions.Hosting;

namespace DocumentationSamples
{


    public static class IgnoreValidationErrors
    {
        public static async Task sample()
        {
            #region sample_IgnoreValidationErrors
            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Advanced.ThrowOnValidationErrors = false;
                }).StartAsync();
            #endregion
        }

    }


}
