namespace Jasper.CommandLine
{
    public enum StartMode
    {
        /// <summary>
        /// Completely builds and starts the underlying IWebHost
        /// </summary>
        Full,

        /// <summary>
        /// Builds, but does not start the underlying IWebHost. Suitable for diagnostic commands
        /// </summary>
        Lightweight
    }
}