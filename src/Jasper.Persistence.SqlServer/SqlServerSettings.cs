namespace Jasper.Persistence.SqlServer
{
    public class SqlServerSettings
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; } = "dbo";

        /// <summary>
        /// The value of the 'database_principal' parameter in calls to APPLOCK_TEST
        /// </summary>
        public string DatabasePrincipal { get; set; } = "dbo";
    }
}
