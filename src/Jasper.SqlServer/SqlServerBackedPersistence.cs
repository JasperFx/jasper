using System;
using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Resiliency;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.SqlServer
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

            // TODO -- use a custom Instance here for a wee bit of optimization
            registry.Services.AddScoped<SqlConnection>(s =>
                new SqlConnection(s.GetService<SqlServerSettings>().ConnectionString));

            registry.Services.AddScoped<DbConnection>(s =>
                new SqlConnection(s.GetService<SqlServerSettings>().ConnectionString));
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
