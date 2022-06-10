using System;
using System.Linq;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Marten;

public static class JasperOptionsMartenExtensions
{
    /// <summary>
    ///     Integrate Marten with Jasper's persistent outbox and add Marten-specific middleware
    ///     to Jasper
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="schemaName">Optionally move the Jasper envelope storage to a separate schema</param>
    /// <returns></returns>
    public static MartenServiceCollectionExtensions.MartenConfigurationExpression IntegrateWithJasper(
        this MartenServiceCollectionExtensions.MartenConfigurationExpression expression, string schemaName = null)
    {
        expression.Services.ConfigureMarten(opts =>
        {
            opts.Storage.Add(new MartenDatabaseSchemaFeature(schemaName ?? opts.DatabaseSchemaName));
        });

        expression.Services.AddSingleton<IEnvelopePersistence, PostgresqlEnvelopePersistence>();
        expression.Services.AddSingleton<IJasperExtension>(new MartenIntegration());

        expression.Services.AddSingleton(s =>
        {
            var store = s.GetRequiredService<IDocumentStore>();

            return new PostgresqlSettings
            {
                // TODO -- this won't work with multi-tenancy
                ConnectionString = store.Storage.Database.CreateConnection().ConnectionString,
                SchemaName = schemaName ?? store.Options.DatabaseSchemaName
            };
        });

        return expression;
    }

    internal static MartenIntegration? FindMartenIntegration(this IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IJasperExtension) && x.ImplementationInstance is MartenIntegration);

        return descriptor?.ImplementationInstance as MartenIntegration;
    }

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression UseEventPublishing(this MartenServiceCollectionExtensions.MartenConfigurationExpression expression, Action<IEventPublishingOptions> configure)
    {
        var integration = expression.Services.FindMartenIntegration();
        if (integration == null)
        {
            expression.IntegrateWithJasper();
            integration = expression.Services.FindMartenIntegration();
        }

        integration!.PublishingConfiguration = configure;

        return expression;
    }

    /// <summary>
    /// Automatically publish all events that are captured in sessions with the Jasper outbox enabled
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static MartenServiceCollectionExtensions.MartenConfigurationExpression PublishAllEvents(
        this MartenServiceCollectionExtensions.MartenConfigurationExpression expression)
    {
        return expression.UseEventPublishing(x => x.PublishAllEvents());
    }
}
