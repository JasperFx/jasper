using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baseline;
using BaselineTypeDiscovery;
using Jasper;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports.Local;
using Jasper.Transports.Stub;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Jasper.Testing")]

namespace Jasper;


/// <summary>
///     Completely defines and configures a Jasper application
/// </summary>
public partial class JasperOptions : IExtensions
{
    protected static Assembly _rememberedCallingAssembly;


    private readonly List<IJasperExtension> _appliedExtensions = new();

    private readonly IList<Type> _extensionTypes = new List<Type>();

    private IMessageSerializer? _defaultSerializer;


    public JasperOptions() : this(null)
    {
    }

    public JasperOptions(string assemblyName)
    {
        Add(new StubTransport());
        Add(new LocalTransport());

        establishApplicationAssembly(assemblyName);

        Advanced = new AdvancedSettings(ApplicationAssembly);

        deriveServiceName();
    }

    /// <summary>
    ///     Apply Jasper extensions
    /// </summary>
    public IExtensions Extensions => this;


    /// <summary>
    ///     Advanced configuration options for Jasper message processing,
    ///     job scheduling, validation, and resiliency features
    /// </summary>
    public AdvancedSettings Advanced { get; }


    /// <summary>
    ///     Register additional services to the underlying IoC container
    /// </summary>
    public ServiceRegistry Services { get; } = new ServiceRegistry();

    /// <summary>
    ///     The main application assembly for this Jasper system
    /// </summary>
    public Assembly ApplicationAssembly { get; internal set; }

    internal HandlerGraph HandlerGraph { get; } = new();

    /// <summary>
    ///     Options to control how Jasper discovers message handler actions, error
    ///     handling, local worker queues, and other policies on message handling
    /// </summary>
    public IHandlerConfiguration Handlers => HandlerGraph;

    /// <summary>
    ///     Read only view of the extensions that have been applied to this
    ///     JasperOptions
    /// </summary>
    public IReadOnlyList<IJasperExtension> AppliedExtensions => _appliedExtensions;

    /// <summary>
    ///     Get or set the logical Jasper service name. By default, this is
    ///     derived from the name of a custom JasperOptions
    /// </summary>
    public string? ServiceName
    {
        get => Advanced.ServiceName;
        set => Advanced.ServiceName = value;
    }

    /// <summary>
    ///     Default message serializers for the application
    /// </summary>
    public IList<IMessageSerializer?> Serializers { get; } =
        new List<IMessageSerializer?> { EnvelopeReaderWriter.Instance };

    public IMessageSerializer? DefaultSerializer
    {
        get
        {
            return _defaultSerializer ??=
                Serializers.FirstOrDefault(x => x.ContentType == EnvelopeConstants.JsonContentType) ??
                Serializers.FirstOrDefault();
        }
        set
        {
            Serializers.Fill(value);
            _defaultSerializer = value;
        }
    }


    /// <summary>
    ///     Applies the extension to this application
    /// </summary>
    /// <param name="extension"></param>
    void IExtensions.Include(IJasperExtension extension)
    {
        ApplyExtensions(new[] { extension });
    }

    /// <summary>
    ///     Applies the extension with optional configuration to the application
    /// </summary>
    /// <param name="configure">Optional configuration of the extension</param>
    /// <typeparam name="T"></typeparam>
    void IExtensions.Include<T>(Action<T>? configure)
    {
        var extension = new T();
        configure?.Invoke(extension);

        ApplyExtensions(new IJasperExtension[] { extension });
    }

    T IExtensions.GetRegisteredExtension<T>()
    {
        return _appliedExtensions.OfType<T>().FirstOrDefault();
    }

    private void deriveServiceName()
    {
        if (GetType() == typeof(JasperOptions))
        {
            Advanced.ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
        }
        else
        {
            Advanced.ServiceName = GetType().Name.Replace("JasperOptions", "").Replace("Registry", "")
                .Replace("Options", "");
        }
    }

    private void establishApplicationAssembly(string assemblyName)
    {
        // TODO -- just use Assembly.GetEntryAssembly()
        if (assemblyName.IsNotEmpty())
        {
            ApplicationAssembly = Assembly.Load(assemblyName);
        }
        else if (GetType() == typeof(JasperOptions) || GetType() == typeof(JasperOptions))
        {
            if (_rememberedCallingAssembly == null)
            {
                _rememberedCallingAssembly = CallingAssembly.DetermineApplicationAssembly(this);
            }

            ApplicationAssembly = _rememberedCallingAssembly;
        }
        else
        {
            ApplicationAssembly = CallingAssembly.DetermineApplicationAssembly(this);
        }

        if (ApplicationAssembly == null)
        {
            throw new InvalidOperationException("Unable to determine an application assembly");
        }
    }

    internal void ApplyExtensions(IJasperExtension[] extensions)
    {
        // Apply idempotency
        extensions = extensions.Where(x => !_extensionTypes.Contains(x.GetType())).ToArray();

        foreach (var extension in extensions)
        {
            extension.Configure(this);
            _appliedExtensions.Add(extension);
        }

        _extensionTypes.Fill(extensions.Select(x => x.GetType()));
    }

    internal void CombineServices(IServiceCollection services)
    {
        services.Clear();
        services.AddRange(Services);
    }

    internal IMessageSerializer DetermineSerializer(Envelope? envelope)
    {
        // TODO -- make this a dictionary for the serializers
        return Serializers.FirstOrDefault(x => x.ContentType.EqualsIgnoreCase(envelope.ContentType)) ??
               DefaultSerializer;
    }
}
