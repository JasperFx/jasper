using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public abstract class SagaFramePolicy : IHandlerPolicy
    {
        public static readonly Type[] ValidSagaIdTypes = new Type[]{typeof(Guid), typeof(int), typeof(long), typeof(string)};

        public const string SagaIdPropertyName = "SagaId";

        public void Apply(HandlerGraph graph)
        {
            throw new System.NotImplementedException();
        }

        public static PropertyInfo ChooseSagaIdProperty(Type messageType)
        {
            var prop = messageType.GetProperties().FirstOrDefault(x => x.HasAttribute<SagaIdentityAttribute>())
                   ?? messageType.GetProperties().FirstOrDefault(x => x.Name == SagaIdPropertyName);

            return prop;
        }

        public static bool IsSagaRelated(HandlerChain chain)
        {
            return chain.Handlers.Any(x => x.HandlerType.Closes(typeof(StatefulSagaOf<>)));
        }

        public abstract Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId,
            Type sagaStateType);
    }


    public enum SagaStateExistence
    {
        New,
        Existing
    }

    public class PullSagaIdFromMessageFrame : SyncFrame
    {
        private readonly Type _messageType;
        private readonly PropertyInfo _sagaIdProperty;
        private Variable _message;
        private Variable _envelope;

        public PullSagaIdFromMessageFrame(Type messageType, PropertyInfo sagaIdProperty)
        {
            _messageType = messageType;
            _sagaIdProperty = sagaIdProperty;

            if (!SagaFramePolicy.ValidSagaIdTypes.Contains(_sagaIdProperty.PropertyType))
            {
                throw new ArgumentOutOfRangeException(nameof(messageType), $"SagaId must be one of {SagaFramePolicy.ValidSagaIdTypes.Select(x => x.NameInCode()).Join(", ")}");
            }

            SagaId = new Variable(_messageType, "sagaId", this);
        }

        public Variable SagaId { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (_sagaIdProperty.PropertyType == typeof(string))
            {
                writer.Write($"{_sagaIdProperty.PropertyType.NameInCode()} sagaId = {_envelope}.{nameof(Envelope.SagaId)} ?? {_message.Usage}.{_sagaIdProperty.Name};");
            }
            else
            {
                var typeNameInCode = _sagaIdProperty.PropertyType.NameInCode();

                writer.Write($"if (!{typeNameInCode}.TryParse({_envelope.Usage}.{nameof(Envelope.SagaId)}, out {typeNameInCode} sagaId)) sagaId = {_message.Usage}.{_sagaIdProperty.Name};");
            }


            // TODO -- set the SagaId on message context?
            Next?.GenerateCode(method, writer);

        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _message = chain.FindVariable(_messageType);
            yield return _message;

            _envelope = chain.FindVariable(typeof(Envelope));
            yield return _envelope;
        }
    }
}
