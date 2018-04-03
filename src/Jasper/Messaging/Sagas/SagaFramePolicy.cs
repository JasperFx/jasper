using System;
using System.Linq;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;

namespace Jasper.Messaging.Sagas
{
    public class SagaFramePolicy : IHandlerPolicy
    {
        public const string SagaIdPropertyName = "SagaId";

        public void Apply(HandlerGraph graph)
        {
            throw new System.NotImplementedException();
        }

        public static PropertyInfo ChooseSagaIdProperty(Type messageType)
        {
            return messageType.GetProperties().FirstOrDefault(x => x.HasAttribute<SagaIdAttribute>())
                   ?? messageType.GetProperties().FirstOrDefault(x => x.Name == SagaIdPropertyName);
        }
    }

    /// <summary>
    /// Marks a public property on a message type as the saga state identity
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SagaIdAttribute : Attribute
    {

    }
}
