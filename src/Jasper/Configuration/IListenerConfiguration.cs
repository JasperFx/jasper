﻿using System;
using System.Threading.Tasks.Dataflow;
using Jasper.Serialization;
using Newtonsoft.Json;

namespace Jasper.Configuration;

public interface IListenerConfiguration<T>
{
    /// <summary>
    ///     Specify the maximum number of threads that this worker queue
    ///     can use at one time
    /// </summary>
    /// <param name="maximumParallelHandlers"></param>
    /// <returns></returns>
    T MaximumThreads(int maximumParallelHandlers);

    /// <summary>
    ///     Forces this worker queue to use no more than one thread
    /// </summary>
    /// <returns></returns>
    T Sequential();

    /// <summary>
    ///     Force any messages enqueued to this worker queue to be durable
    /// </summary>
    /// <returns></returns>
    T DurablyPersistedLocally();

    /// <summary>
    ///     Incoming messages are immediately moved into an in-memory queue
    ///     for parallel processing
    /// </summary>
    /// <returns></returns>
    T BufferedInMemory();


    /// <summary>
    ///     Incoming messages are executed in
    /// </summary>
    /// <returns></returns>
    T ProcessInline();


    /// <summary>
    ///     Fine tune the internal message handling queue for this listener
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    T ConfigureExecution(Action<ExecutionDataflowBlockOptions> configure);


    /// <summary>
    ///     Mark this listener as the preferred endpoint for replies from other systems
    /// </summary>
    /// <returns></returns>
    T UseForReplies();

    /// <summary>
    ///     Give this subscriber endpoint a diagnostic name. This will be used
    ///     by Open Telemetry diagnostics if set
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    T Named(string name);

    /// <summary>
    ///     Use custom Newtonsoft.Json settings for this listener endpoint
    /// </summary>
    /// <param name="customSettings"></param>
    /// <returns></returns>
    T CustomNewtonsoftJsonSerialization(JsonSerializerSettings customSettings);

    /// <summary>
    ///     Override the default serializer for this endpoint
    /// </summary>
    /// <param name="serializer"></param>
    /// <returns></returns>
    T DefaultSerializer(IMessageSerializer serializer);
}

public interface IListenerConfiguration : IListenerConfiguration<IListenerConfiguration>
{
}
