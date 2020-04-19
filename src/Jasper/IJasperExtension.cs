namespace Jasper
{
    // SAMPLE: IJasperExtension
    /// <summary>
    ///     Use to create loadable extensions to Jasper applications
    /// </summary>
    public interface IJasperExtension
    {
        /// <summary>
        ///     Make any alterations to the JasperOptions for the application
        /// </summary>
        /// <param name="options"></param>
        void Configure(JasperOptions options);
    }

    // ENDSAMPLE
}
