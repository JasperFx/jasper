using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Configuration
{
    public interface ISubscriber
    {
        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <typeparam name="TModifier"></typeparam>
        /// <returns></returns>
        ISubscriber ModifyWith<TModifier>() where TModifier : IEnvelopeModifier, new();

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        ISubscriber ModifyWith(IEnvelopeModifier modifier);
    }
}
