using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            MapPropertyToHeader(x => x.Attempts, EnvelopeSerializer.AttemptsKey);
            MapPropertyToHeader(x => x.AckRequested, EnvelopeSerializer.AckRequestedKey);
            MapPropertyToHeader(x => x.MessageType, EnvelopeSerializer.MessageTypeKey);
            MapPropertyToHeader(x => x.AcceptedContentTypes, EnvelopeSerializer.AcceptedContentTypesKey);

            // TODO -- could check it here, then delete it on the spot instead of mapping it!!
            MapPropertyToHeader(x => x.DeliverBy, EnvelopeSerializer.DeliverByHeader);

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

            var getUri = GetType().GetMethod(nameof(readUri));
            var getInt = GetType().GetMethod(nameof(readInt));
            var getString = GetType().GetMethod(nameof(readString));
            var getGuid = GetType().GetMethod(nameof(readGuid));
            var getBoolean = GetType().GetMethod(nameof(readBoolean));
            var getDateTimeOffset = GetType().GetMethod(nameof(readDateTimeOffset));

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

                var setter = pair.Key.SetMethod;

                var getValue = Expression.Call(protocol, getMethod, incoming, Expression.Constant(pair.Value));
                var setValue = Expression.Call(envelope, setter, getValue);

                list.Add(setValue);

            }

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

            var setUri = GetType().GetMethod(nameof(writeUri));
            var setInt = GetType().GetMethod(nameof(writeInt));
            var setString = GetType().GetMethod(nameof(writeOutgoingHeader));
            var setGuid = GetType().GetMethod(nameof(writeGuid));
            var setBoolean = GetType().GetMethod(nameof(writeBoolean));
            var setDateTimeOffset = GetType().GetMethod(nameof(writeDateTimeOffset));

            var list = new List<Expression>();

            foreach (var pair in _envelopeToHeader.Where(x => !_envelopeToIncoming.ContainsKey(x.Key)))
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


                var getEnvelopeValue = Expression.Call(envelope, pair.Key.GetMethod);
                var setOutgoingValue = Expression.Call(protocol, setMethod, outgoing, Expression.Constant(pair.Value), getEnvelopeValue);

                list.Add(setOutgoingValue);

            }

            var block = Expression.Block(list);

            var lambda = Expression.Lambda<Action<Envelope, TOutgoing>>(block, envelope, outgoing);

            // TODO -- add FastExpressionCompiler!!!
            return lambda.Compile();
        }



        public void MapIncoming(Envelope envelope, TIncoming incoming)
        {
            _mapIncoming.Value(envelope, incoming);
        }

        public void MapOutgoing(Envelope envelope, TOutgoing outgoing)
        {
            _mapOutgoing.Value(envelope, outgoing);
        }

        protected abstract void writeOutgoingHeader(TOutgoing outgoing, string key, string value);
        protected abstract bool tryReadIncomingHeader(TIncoming incoming, string key, out string value);

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

        protected void writeUri(TOutgoing outgoing, string key, Uri value)
        {
            if (value != null)
            {
                writeOutgoingHeader(outgoing, key, value.ToString());
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
                writeOutgoingHeader(outgoing, key, value.ToString());
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
