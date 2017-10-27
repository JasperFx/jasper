﻿using System;
using System.IO;
using Baseline;
using Jasper.Bus;

namespace Jasper.Conneg
{
    // SAMPLE: ISerializer
    public interface ISerializerFactory
    {
        object Deserialize(Stream message);

        string ContentType { get; }

        IMessageDeserializer[] ReadersFor(Type messageType, MediaSelectionMode mode);
        IMessageSerializer[] WritersFor(Type messageType, MediaSelectionMode mode);
        IMessageDeserializer VersionedReaderFor(Type incomingType);
    }
    // ENDSAMPLE
}
