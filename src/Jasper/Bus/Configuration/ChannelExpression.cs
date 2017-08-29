using Jasper.Bus.Runtime;

namespace Jasper.Bus.Configuration
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

        public ChannelExpression MaximumParallelization(int maximumParallelHandlers    )
        {
            _node.MaximumParallelization = maximumParallelHandlers;
            return this;
        }
    }
}
