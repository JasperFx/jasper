using System;
using Baseline;
using Marten;

namespace JasperBus.Marten.Tests.Setup
{
    public class ConnectionSource : ConnectionFactory
    {
        public static readonly string ConnectionString = Environment.GetEnvironmentVariable("marten-testing-database");

        static ConnectionSource()
        {
            if (ConnectionString.IsEmpty())
                throw new Exception(
                    "You need to set the connection string for your local Postgresql database in the environment variable 'marten-testing-database'");
        }

        public ConnectionSource() : base(ConnectionString)
        {
        }
    }
}
