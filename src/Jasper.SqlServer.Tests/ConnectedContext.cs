using Jasper.SqlServer.Schema;

namespace Jasper.SqlServer.Tests
{
    public abstract class ConnectedContext
    {
        public ConnectedContext()
        {
            var loader = new SchemaLoader(ConnectionSource.ConnectionString);
            loader.RecreateAll();
        }
    }
}
