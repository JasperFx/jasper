using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Serialization;

namespace Jasper.Transports
{
    public abstract class Protocol<TIncoming, TOutgoing>
    {
        private readonly Lazy<Action<Envelope, TIncoming>> _mapIncoming;
        private readonly Lazy<Action<Envelope, TOutgoing>> _mapOutgoing;

        private readonly Dictionary<PropertyInfo, PropertyInfo> _envelopeToIncoming =
            new Dictionary<PropertyInfo, PropertyInfo>();

        private readonly Dictionary<PropertyInfo, PropertyInfo> _envelopeToOutgoing =
            new Dictionary<PropertyInfo, PropertyInfo>();

        private readonly Dictionary<PropertyInfo, string> _envelopeToHeader = new Dictionary<PropertyInfo, string>();

        public Protocol()
        {
            _mapIncoming = new Lazy<Action<Envelope, TIncoming>>(compileIncoming);
            _mapOutgoing = new Lazy<Action<Envelope, TOutgoing>>(compileOutgoing);

            MapPropertyToHeader(x => x.CorrelationId, EnvelopeSerializer.CorrelationIdKey);
            MapPropertyToHeader(x => x.SagaId, EnvelopeSerializer.SagaIdKey);
            MapPropertyToHeader(x => x.Id, EnvelopeSerializer.IdKey);
            MapPropertyToHeader(x => x.CausationId, EnvelopeSerializer.CausationIdKey);
            MapPropertyToHeader(x => x.ContentType, EnvelopeSerializer.ContentTypeKey);
            MapPropertyToHeader(x => x.Source, EnvelopeSerializer.SourceKey);
            MapPropertyToHeader(x => x.ReplyRequested, EnvelopeSerializer.ReplyRequestedKey);
            MapPropertyToHeader(x => x.ReplyUri, EnvelopeSerializer.ReplyUriKey);
            MapPropertyToHeader(x => x.ExecutionTime, EnvelopeSerializer.ExecutionTimeKey);

            MapPropertyToHeader(x => x.AckRequested, EnvelopeSerializer.AckRequestedKey);
            MapPropertyToHeader(x => x.MessageType, EnvelopeSerializer.MessageTypeKey);
            MapPropertyToHeader(x => x.AcceptedContentTypes, EnvelopeSerializer.AcceptedContentTypesKey);

            // TODO -- could check it here, then delete it on the spot instead of mapping it!!
            MapPropertyToHeader(x => x.DeliverBy, EnvelopeSerializer.DeliverByHeader);

            MapPropertyToHeader(x => x.Attempts, EnvelopeSerializer.AttemptsKey);

        }

        public void MapPropertyToHeader(Expression<Func<Envelope, object>> property, string headerKey)
        {
            var prop = ReflectionHelper.GetProperty(property);
            _envelopeToHeader[prop] = headerKey;
        }

        private Action<Envelope, TIncoming> compileIncoming()
        {
            var incoming = Expression.Parameter(typeof(TIncoming), "incoming");
            var envelope = Expression.Parameter(typeof(Envelope), "env");
            var protocol = Expression.Constant(this);

            var getUri = GetType().GetMethod(nameof(readUri), BindingFlags.NonPublic | BindingFlags.Instance);
            var getInt = GetType().GetMethod(nameof(readInt), BindingFlags.NonPublic | BindingFlags.Instance);
            var getString = GetType().GetMethod(nameof(readString), BindingFlags.NonPublic | BindingFlags.Instance);
            var getGuid = GetType().GetMethod(nameof(readGuid), BindingFlags.NonPublic | BindingFlags.Instance);
            var getBoolean = GetType().GetMethod(nameof(readBoolean), BindingFlags.NonPublic | BindingFlags.Instance);
            var getDateTimeOffset = GetType().GetMethod(nameof(readDateTimeOffset), BindingFlags.NonPublic | BindingFlags.Instance);
            var getStringArray = GetType().GetMethod(nameof(readStringArray), BindingFlags.NonPublic | BindingFlags.Instance);

            var list = new List<Expression>();

            foreach (var pair in _envelopeToHeader.Where(x => !_envelopeToOutgoing.ContainsKey(x.Key)))
            {
                MethodInfo getMethod = getString;
                if (pair.Key.PropertyType == typeof(Uri))
                {
                    getMethod = getUri;
                }
                else if (pair.Key.PropertyType == typeof(Guid))
                {
                    getMethod = getGuid;
                }
                else if (pair.Key.PropertyType == typeof(bool))
                {
                    getMethod = getBoolean;
                }
                else if (pair.Key.PropertyType == typeof(DateTimeOffset?))
                {
                    getMethod = getDateTimeOffset;
                }
                else if (pair.Key.PropertyType == typeof(int))
                {
                    getMethod = getInt;
                }
                else if (pair.Key.PropertyType == typeof(string[]))
                {
                    getMethod = getStringArray;
                }

                var setter = pair.Key.SetMethod;

                var getValue = Expression.Call(protocol, getMethod, incoming, Expression.Constant(pair.Value));
                var setValue = Expression.Call(envelope, setter, getValue);

                list.Add(setValue);

            }

            var writeHeaders = Expression.Call(protocol,
                GetType().GetMethod(nameof(writeIncomingHeaders), BindingFlags.NonPublic | BindingFlags.Instance),
                incoming, envelope);
            list.Add(writeHeaders);

            var block = Expression.Block(list);

            var lambda = Expression.Lambda<Action<Envelope, TIncoming>>(block, envelope, incoming);

            // TODO -- add FastExpressionCompiler!!!
            return lambda.Compile();
        }

        private Action<Envelope, TOutgoing> compileOutgoing()
        {
            var outgoing = Expression.Parameter(typeof(TOutgoing), "outgoing");
            var envelope = Expression.Parameter(typeof(Envelope), "env");
            var protocol = Expression.Constant(this);

            var setUri = GetType().GetMethod(nameof(writeUri), BindingFlags.NonPublic | BindingFlags.Instance);
            var setInt = GetType().GetMethod(nameof(writeInt), BindingFlags.NonPublic | BindingFlags.Instance);
            var setString = GetType().GetMethod(nameof(writeString), BindingFlags.NonPublic | BindingFlags.Instance);
            var setGuid = GetType().GetMethod(nameof(writeGuid), BindingFlags.NonPublic | BindingFlags.Instance);
            var setBoolean = GetType().GetMethod(nameof(writeBoolean), BindingFlags.NonPublic | BindingFlags.Instance);
            var setDateTimeOffset = GetType().GetMethod(nameof(writeDateTimeOffset), BindingFlags.NonPublic | BindingFlags.Instance);
            var setStringArray = GetType().GetMethod(nameof(writeStringArray), BindingFlags.NonPublic | BindingFlags.Instance);

            var list = new List<Expression>();

            var headers = _envelopeToHeader.Where(x => !_envelopeToIncoming.ContainsKey(x.Key));
            foreach (var pair in headers)
            {
                MethodInfo setMethod = setString;
                if (pair.Key.PropertyType == typeof(Uri))
                {
                    setMethod = setUri;
                }
                else if (pair.Key.PropertyType == typeof(Guid))
                {
                    setMethod = setGuid;
                }
                else if (pair.Key.PropertyType == typeof(bool))
                {
                    setMethod = setBoolean;
                }
                else if (pair.Key.PropertyType == typeof(DateTimeOffset?))
                {
                    setMethod = setDateTimeOffset;
                }
                else if (pair.Key.PropertyType == typeof(int))
                {
                    setMethod = setInt;
                }
                else if (pair.Key.PropertyType == typeof(string[]))
                {
                    setMethod = setStringArray;
                }

                var getEnvelopeValue = Expression.Call(envelope, pair.Key.GetMethod);
                var setOutgoingValue = Expression.Call(protocol, setMethod, outgoing, Expression.Constant(pair.Value), getEnvelopeValue);

                list.Add(setOutgoingValue);

            }

            var writeHeaders = Expression.Call(protocol,
                GetType().GetMethod(nameof(writeOutgoingOtherHeaders), BindingFlags.NonPublic | BindingFlags.Instance),
                outgoing, envelope);
            list.Add(writeHeaders);

            var block = Expression.Block(list);

            var lambda = Expression.Lambda<Action<Envelope, TOutgoing>>(block, envelope, outgoing);

            // TODO -- add FastExpressionCompiler!!!
            return lambda.Compile();
        }


        protected void writeOutgoingOtherHeaders(TOutgoing outgoing, Envelope envelope)
        {
            var reserved = _envelopeToHeader.Values.ToArray();

            foreach (var header in envelope.Headers.Where(x => !reserved.Contains(x.Key)))
            {
                writeOutgoingHeader(outgoing, header.Key, header.Value);
            }
        }

        public void MapIncomingToEnvelope(Envelope envelope, TIncoming incoming)
        {
            _mapIncoming.Value(envelope, incoming);
        }

        public void MapEnvelopeToOutgoing(Envelope envelope, TOutgoing outgoing)
        {
            _mapOutgoing.Value(envelope, outgoing);
        }

        protected abstract void writeOutgoingHeader(TOutgoing outgoing, string key, string value);
        protected abstract bool tryReadIncomingHeader(TIncoming incoming, string key, out string value);

        /// <summary>
        /// This is strictly for "other" headers that are passed along that are not
        /// used by Jasper
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="envelope"></param>
        protected virtual void writeIncomingHeaders(TIncoming incoming, Envelope envelope)
        {
            // nothing
        }

        protected string[] readStringArray(TIncoming incoming, string key)
        {
            return tryReadIncomingHeader(incoming, key, out var value)
                ? value.Split(',')
                : Array.Empty<string>();
        }

        protected int readInt(TIncoming incoming, string key)
        {
            if (tryReadIncomingHeader(incoming, key, out var raw))
            {
                if (int.TryParse(raw, out var number))
                {
                    return number;
                }
            }

            return default;
        }

        protected string readString(TIncoming incoming, string key)
        {
            return tryReadIncomingHeader(incoming, key, out var value)
                ? value
                : null;
        }

        protected Uri readUri(TIncoming incoming, string key)
        {
            return tryReadIncomingHeader(incoming, key, out var value)
                ? new Uri(value)
                : null;
        }

        protected void writeStringArray(TOutgoing outgoing, string key, string[] value)
        {
            if (value != null)
            {
                writeOutgoingHeader(outgoing, key, value.Join(","));
            }
        }

        protected void writeUri(TOutgoing outgoing, string key, Uri value)
        {
            if (value != null)
            {
                writeOutgoingHeader(outgoing, key, value.ToString());
            }
        }

        protected void writeString(TOutgoing outgoing, string key, string value)
        {
            if (value != null)
            {
                writeOutgoingHeader(outgoing, key, value);
            }
        }

        protected void writeInt(TOutgoing outgoing, string key, int value)
        {
            writeOutgoingHeader(outgoing, key, value.ToString());
        }

        protected void writeGuid(TOutgoing outgoing, string key, Guid value)
        {
            if (value != Guid.Empty)
            {
                writeOutgoingHeader(outgoing, key, value.ToString());
            }
        }

        protected void writeBoolean(TOutgoing outgoing, string key, bool value)
        {
            if (value)
            {
                writeOutgoingHeader(outgoing, key, "true");
            }
        }

        protected void writeDateTimeOffset(TOutgoing outgoing, string key, DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                writeOutgoingHeader(outgoing, key, value.ToString());
            }
        }

        protected Guid readGuid(TIncoming incoming, string key)
        {
            if (tryReadIncomingHeader(incoming, key, out var raw))
            {
                if (Guid.TryParse(raw, out var uuid)) return uuid;
            }

            return Guid.Empty;
        }

        protected bool readBoolean(TIncoming incoming, string key)
        {
            if (tryReadIncomingHeader(incoming, key, out var raw))
            {
                if (bool.TryParse(raw, out var flag)) return flag;
            }

            return false;
        }

        protected DateTimeOffset? readDateTimeOffset(TIncoming incoming, string key)
        {
            if (tryReadIncomingHeader(incoming, key, out var raw))
            {
                if (DateTimeOffset.TryParse(raw, out var flag)) return flag;
            }

            return null;
        }
    }

    public abstract class Protocol<T> : Protocol<T, T>
    {

    }
}
