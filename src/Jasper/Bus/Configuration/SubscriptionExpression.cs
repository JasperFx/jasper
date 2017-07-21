using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Configuration
{
    public class SubscriptionExpression
    {
        private readonly ServiceBusFeature _parent;
        private readonly Uri _receiving;

        public SubscriptionExpression(ServiceBusFeature parent, Uri receiving)
        {
            _parent = parent;
            _receiving = receiving;

            parent.Services.AddType(typeof(ISubscriptionRequirements), typeof(SubscriptionRequirements));
        }

        /// <summary>
        ///     Specify the publishing source of the events you want to subscribe to
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <returns></returns>
        public TypeSubscriptionExpression ToSource(string sourceProperty)
        {
            return ToSource(sourceProperty.ToUri());
        }

        /// <summary>
        ///     Specify the publishing source of the events you want to subscribe to
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <returns></returns>
        public TypeSubscriptionExpression ToSource(Uri sourceProperty)
        {
            var requirement = _receiving == null
                ? (ISubscriptionRequirement) new LocalSubscriptionRequirement(sourceProperty)
                : new GroupSubscriptionRequirement(sourceProperty, _receiving);

            _parent.Services.AddService(requirement);

            return new TypeSubscriptionExpression(requirement);
        }

        public class TypeSubscriptionExpression
        {
            private readonly ISubscriptionRequirement _requirement;

            public TypeSubscriptionExpression(ISubscriptionRequirement requirement)
            {
                _requirement = requirement;
            }

            public TypeSubscriptionExpression ToMessage<TMessage>()
            {
                _requirement.AddType(typeof(TMessage));

                return this;
            }

            public TypeSubscriptionExpression ToMessage(Type messageType)
            {
                _requirement.AddType(messageType);
                return this;
            }
        }
    }
}