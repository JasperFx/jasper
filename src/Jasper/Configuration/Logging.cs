namespace Jasper.Configuration
{
    public class Logging
    {
        internal Logging(JasperRegistry parent)
        {
            Parent = parent;
        }

        internal JasperRegistry Parent { get; }

        public bool UseConsoleLogging { get; set; } = false;
    }
}
