using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Jasper.Runtime;
using Newtonsoft.Json;

namespace Jasper.Serialization;

public class MassTransitJsonSerializer : IMessageSerializer
{
    // TODO -- copy settings from MT
    private readonly IMessageSerializer _inner = new NewtonsoftSerializer(new JsonSerializerSettings());

    public string ContentType { get; } = "application/vnd.masstransit+json";
    public byte[] Write(object message)
    {
        throw new NotImplementedException();
    }

    public object ReadFromData(Type messageType, byte[] data)
    {
        var wrappedType = typeof(JsonMessageEnvelope<>).MakeGenericType(messageType);
        return _inner.ReadFromData(wrappedType, data);
    }

    public object ReadFromData(byte[] data)
    {
        throw new NotImplementedException();
    }

    [Serializable]
    public class JsonMessageEnvelope<T>
    {
        Dictionary<string, object?>? _headers;

        public JsonMessageEnvelope()
        {
        }

        public string? MessageId { get; set; }
        public string? RequestId { get; set; }
        public string? CorrelationId { get; set; }
        public string? ConversationId { get; set; }
        public string? InitiatorId { get; set; }
        public string? SourceAddress { get; set; }
        public string? DestinationAddress { get; set; }
        public string? ResponseAddress { get; set; }
        public string? FaultAddress { get; set; }
        public string[]? MessageType { get; set; }
        public T? Message { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public DateTime? SentTime { get; set; }

        public Dictionary<string, object?> Headers
        {
            get => _headers ??= new Dictionary<string, object?>();
            set => _headers = value;
        }

        public BusHostInfo? Host { get; set; }

    }

    // TODO -- have this memoized.
    [Serializable]
    public class BusHostInfo
    {
        public BusHostInfo()
        {
        }

        public BusHostInfo(bool initialize)
        {
            FrameworkVersion = Environment.Version.ToString();
            OperatingSystemVersion = Environment.OSVersion.ToString();
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetCallingAssembly();
            MachineName = Environment.MachineName;
            MassTransitVersion = typeof(BusHostInfo).GetTypeInfo().Assembly.GetName().Version?.ToString();

            try
            {
                using var currentProcess = Process.GetCurrentProcess();
                ProcessId = currentProcess.Id;
                ProcessName = currentProcess.ProcessName;
                if ("dotnet".Equals(ProcessName, StringComparison.OrdinalIgnoreCase))
                    ProcessName = GetUsefulProcessName(ProcessName);
            }
            catch (PlatformNotSupportedException)
            {
                ProcessId = 0;
                ProcessName = GetUsefulProcessName("UWP");
            }

            var assemblyName = entryAssembly.GetName();
            Assembly = assemblyName.Name;
            AssemblyVersion = assemblyName.Version?.ToString() ?? "Unknown";
        }

        public string? MachineName { get; set; }
        public string? ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string? Assembly { get; set; }
        public string? AssemblyVersion { get; set; }
        public string? FrameworkVersion { get; set; }
        public string? MassTransitVersion { get; set; }
        public string? OperatingSystemVersion { get; set; }

        static string GetAssemblyFileVersion(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (attribute != null)
                return attribute.Version;

            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "Unknown";
        }

        static string GetAssemblyInformationalVersion(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute != null)
                return attribute.InformationalVersion;

            return GetAssemblyFileVersion(assembly);
        }

        static string GetUsefulProcessName(string defaultProcessName)
        {
            var entryAssemblyLocation = System.Reflection.Assembly.GetEntryAssembly()?.Location;

            return string.IsNullOrWhiteSpace(entryAssemblyLocation)
                ? defaultProcessName
                : Path.GetFileNameWithoutExtension(entryAssemblyLocation);
        }
    }
}
