using Jasper.Persistence.SqlServer.Schema;

namespace Jasper.Persistence.SqlServer
{
    public static class JasperRuntimeExtensions
    {
        /// <summary>
        ///     Drops and recreates the Sql Server backed persistence database objects
        /// </summary>
        /// <param name="runtime"></param>
        public static void RebuildMessageStorage(this JasperRuntime runtime)
        {
            var settings = runtime.Get<SqlServerSettings>();
            var builder = new SchemaLoader(settings.ConnectionString, settings.SchemaName);
            builder.RecreateAll();
        }
    }
}
