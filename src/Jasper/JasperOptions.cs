﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Baseline;
using Baseline.Dates;
using BaselineTypeDiscovery;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports.Local;
using Jasper.Transports.Stub;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Jasper.Testing")]

namespace Jasper;

/// <summary>
///     Completely defines and configures a Jasper application
/// </summary>
public sealed partial class JasperOptions : IExtensions
{
    private static Assembly? _rememberedCallingAssembly;
    private readonly List<IJasperExtension> _appliedExtensions = new();
    private readonly IList<Type> _extensionTypes = new List<Type>();

    private readonly IDictionary<string, IMessageSerializer>
        _serializers = new Dictionary<string, IMessageSerializer>();

    private IMessageSerializer? _defaultSerializer;


    public JasperOptions() : this(null)
    {
    }

    public JasperOptions(string? assemblyName)
    {
        _serializers.Add(EnvelopeReaderWriter.Instance.ContentType, EnvelopeReaderWriter.Instance);

        UseNewtonsoftForSerialization();

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
    ///     The default message execution timeout. This uses a CancellationTokenSource
    ///     behind the scenes, and the timeout enforcement is dependent on the usage within handlers
    /// </summary>
    public TimeSpan DefaultExecutionTimeout { get; set; } = 60.Seconds();


    /// <summary>
    ///     Register additional services to the underlying IoC container
    /// </summary>
    public ServiceRegistry Services { get; } = new();

    /// <summary>
    ///     The main application assembly for this Jasper system
    /// </summary>
    public Assembly? ApplicationAssembly { get; internal set; }

    internal HandlerGraph HandlerGraph { get; } = new();

    /// <summary>
    ///     Options to control how Jasper discovers message handler actions, error
    ///     handling, local worker queues, and other policies on message handling
    /// </summary>
    public IHandlerConfiguration Handlers => HandlerGraph;

    /// <summary>
    ///     Get or set the logical Jasper service name. By default, this is
    ///     derived from the name of a custom JasperOptions
    /// </summary>
    public string? ServiceName
    {
        get => Advanced.ServiceName;
        set => Advanced.ServiceName = value;
    }

    public IMessageSerializer DefaultSerializer
    {
        get
        {
            return (_defaultSerializer ??=
                _serializers.Values.FirstOrDefault(x => x.ContentType == EnvelopeConstants.JsonContentType) ??
                _serializers.Values.First());
        }
        set
        {
            if (value == null)
            {
                throw new InvalidOperationException("The DefaultSerializer cannot be null");
            }

            _serializers[value.ContentType] = value;
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

    T? IExtensions.GetRegisteredExtension<T>() where T : default
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

    private void establishApplicationAssembly(string? assemblyName)
    {
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

    internal IMessageSerializer DetermineSerializer(Envelope envelope)
    {
        if (envelope.ContentType.IsEmpty())
        {
            return DefaultSerializer;
        }

        if (_serializers.TryGetValue(envelope.ContentType, out var serializer))
        {
            return serializer;
        }

        return DefaultSerializer;
    }

    /// <summary>
    ///     Use Newtonsoft.Json as the default JSON serialization with optional configuration
    /// </summary>
    /// <param name="configuration"></param>
    public void UseNewtonsoftForSerialization(Action<JsonSerializerSettings>? configuration = null)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        configuration?.Invoke(settings);

        var serializer = new NewtonsoftSerializer(settings);

        _serializers[serializer.ContentType] = serializer;
    }

    /// <summary>
    ///     Use System.Text.Json as the default JSON serialization with optional configuration
    /// </summary>
    /// <param name="configuration"></param>
    public void UseSystemTextJsonForSerialization(Action<JsonSerializerOptions>? configuration = null)
    {
        var options = new JsonSerializerOptions();

        configuration?.Invoke(options);

        var serializer = new SystemTextJsonSerializer(options);

        _serializers[serializer.ContentType] = serializer;
    }

    internal void IncludeExtensionAssemblies(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies) HandlerGraph.Source.IncludeAssembly(assembly);
    }

    public IMessageSerializer FindSerializer(string contentType)
    {
        if (_serializers.TryGetValue(contentType, out var serializer))
        {
            return serializer;
        }

        throw new ArgumentOutOfRangeException(nameof(contentType));
    }

    /// <summary>
    ///     Register an alternative serializer with this Jasper application
    /// </summary>
    /// <param name="serializer"></param>
    public void AddSerializer(IMessageSerializer serializer)
    {
        _serializers[serializer.ContentType] = serializer;
    }
}
