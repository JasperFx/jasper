using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class SubscriptionRequirement : ISubscriptionExpression
    {
        public readonly IList<Type> MessageTypes = new List<Type>();
        public readonly IList<Func<Type, bool>> Includes = new List<Func<Type, bool>>();

        public Uri ReceiverLocation { get; set; } // could be a default here

        public IEnumerable<Subscription> DetermineSubscriptions(SerializationGraph serialization, Type[] allTypes, Uri defaultReceiver)
        {
            var receiver = ReceiverLocation ?? defaultReceiver;

            return Includes.SelectMany(allTypes.Where)
                .Concat(MessageTypes)
                .Distinct()
                .Select(type =>
                {
                    var reader = serialization.ReaderFor(type);
                    return new Subscription(type, receiver)
                    {
                        Accept = reader.ContentTypes
                    };
                });
        }


        void ISubscriptionReceiverExpression.At(Uri receiver)
        {
            ReceiverLocation = receiver;
        }

        void ISubscriptionReceiverExpression.At(string receiverUriString)
        {
            ReceiverLocation = receiverUriString.ToUri();
        }

        ISubscriptionExpression ISubscriptionExpression.To<T>()
        {
            MessageTypes.Add(typeof(T));
            return this;
        }

        ISubscriptionExpression ISubscriptionExpression.To(Type messageType)
        {
            MessageTypes.Add(messageType);
            return this;
        }

        ISubscriptionExpression ISubscriptionExpression.To(Func<Type, bool> filter)
        {
            Includes.Add(filter);
            return this;
        }
    }
}
