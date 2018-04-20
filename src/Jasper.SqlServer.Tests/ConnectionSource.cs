using System;
using Baseline;

namespace Jasper.SqlServer.Tests
{
    public static class ConnectionSource
    {
        public static readonly string Default = "Server=localhost;Database=jasper_testing;User Id=sa;Password=P@55w0rd";

        public static readonly string ConnectionString = Environment.GetEnvironmentVariable("sqlserver_testing_database") ?? Default;

        static ConnectionSource()
        {
            if (ConnectionString.IsEmpty())
                throw new Exception(
                    "You need to set the connection string for your local Postgresql database in the environment variable 'marten_testing_database'");
        }


    }
}
