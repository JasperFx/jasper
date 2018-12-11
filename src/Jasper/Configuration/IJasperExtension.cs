namespace Jasper.Configuration
{
    // SAMPLE: IJasperExtension
    /// <summary>
    ///     Use to create loadable extensions to Jasper applications
    /// </summary>
    public interface IJasperExtension
    {
        /// <summary>
        ///     Make any alterations to the JasperRegistry for the application
        /// </summary>
        /// <param name="registry"></param>
        void Configure(JasperOptionsBuilder registry);
    }

    // ENDSAMPLE
}
