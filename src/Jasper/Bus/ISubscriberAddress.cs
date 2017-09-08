using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public interface ISubscriberAddress
    {
        // TODO -- allow users to specify the outgoing batch size here

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <typeparam name="TModifier"></typeparam>
        /// <returns></returns>
        ISubscriberAddress ModifyWith<TModifier>() where TModifier : IEnvelopeModifier, new();

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        ISubscriberAddress ModifyWith(IEnvelopeModifier modifier);
    }
}