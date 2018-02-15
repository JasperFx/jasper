namespace Jasper.Configuration
{
    public class Logging
    {
        internal Logging(JasperRegistry parent)
        {
            Parent = parent;
        }

        internal JasperRegistry Parent { get; }

        /// <summary>
        /// Opt into verbose, console logging of message or Http handling events
        /// </summary>
        public bool UseConsoleLogging { get; set; } = false;

        public bool Verbose { get; set; }

    }
}
