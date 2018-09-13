using System;
using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Resiliency;
using Jasper.Persistence.SqlServer.Util;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{
    /// <summary>
    /// Activates the Sql Server backed message persistence
    /// </summary>
    public class SqlServerBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<SqlServerSettings>();

            registry.Services.AddSingleton<IDurableMessagingFactory, SqlServerBackedDurableMessagingFactory>();
            registry.Services.AddTransient<IEnvelopePersistor, SqlServerEnvelopePersistor>();

            registry.Services.AddSingleton<IHostedService, SchedulingAgent>();

            registry.CodeGeneration.Sources.Add(new SqlServerBackedPersistenceMarker());


            registry.Services.For<SqlConnection>().Use<SqlConnection>();

            registry.Services.Add(new SqlConnectionInstance(typeof(SqlConnection)));
            registry.Services.Add(new SqlConnectionInstance(typeof(DbConnection)));

            registry.CodeGeneration.Transactions = new SqlServerTransactionFrameProvider();

        }
    }

    internal static class MethodVariablesExtensions
    {
        internal static bool IsUsingSqlServerPersistence(this IMethodVariables method)
        {
            return method.TryFindVariable(typeof(SqlServerBackedPersistenceMarker), VariableSource.NotServices) != null;
        }
    }

    internal class SqlServerBackedPersistenceMarker : IVariableSource
    {
        public bool Matches(Type type)
        {
            return type == GetType();
        }

        public Variable Create(Type type)
        {
            return Variable.For<SqlServerBackedDurableMessagingFactory>();
        }
    }
}
