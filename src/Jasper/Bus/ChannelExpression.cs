using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public class ChannelExpression
    {
        private readonly ChannelGraph _channels;
        private readonly ChannelNode _node;

        internal ChannelExpression(ChannelGraph channels, ChannelNode node)
        {
            _channels = channels;
            _node = node;
        }


        public ChannelExpression UseAsControlChannel()
        {
            _node.Incoming = true;
            _channels.ControlChannel = _node;

            return this;
        }

        /// <summary>
        /// Require this channel to be guaranteed delivery
        /// </summary>
        /// <returns></returns>
        public ChannelExpression DeliveryGuaranteed()
        {
            _node.Mode = DeliveryMode.DeliveryGuaranteed;
            return this;
        }

        /// <summary>
        /// Opt out of guaranteed delivery for a faster, but unsafe transport. Suitable for control queues
        /// </summary>
        /// <returns></returns>
        public ChannelExpression DeliveryFastWithoutGuarantee()
        {
            _node.Mode = DeliveryMode.DeliveryFastWithoutGuarantee;
            return this;
        }

        /// <summary>
        /// Alter the sending and receiving mode of this channel
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public ChannelExpression Mode(DeliveryMode mode)
        {
            _node.Mode = mode;
            return this;
        }

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <typeparam name="TModifier"></typeparam>
        /// <returns></returns>
        public ChannelExpression ModifyWith<TModifier>() where TModifier : IEnvelopeModifier, new()
        {
            return ModifyWith(new TModifier());
        }

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public ChannelExpression ModifyWith(IEnvelopeModifier modifier)
        {
            _node.Modifiers.Add(modifier);

            return this;
        }

        public ChannelExpression DefaultContentType(string contentType)
        {
            if (_node.AcceptedContentTypes.Contains(contentType))
            {
                _node.AcceptedContentTypes.Remove(contentType);
            }

            _node.AcceptedContentTypes.Insert(0, contentType);

            return this;
        }

        public ChannelExpression AcceptedContentTypes(params string[] contentTypes)
        {
            _node.AcceptedContentTypes.Clear();
            _node.AcceptedContentTypes.AddRange(contentTypes);
                
            return this;
        }
    }
}