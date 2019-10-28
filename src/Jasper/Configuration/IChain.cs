using System;
using System.Collections.Generic;
using System.Linq;
using Lamar;
using LamarCodeGeneration.Frames;

namespace Jasper.Configuration
{
    // SAMPLE: IChain
    /// <summary>
    /// Models the middleware arrangement for either an HTTP route execution
    /// or the execution of a message
    /// </summary>
    public interface IChain
    {
        /// <summary>
        /// Frames that would be initially placed in front of
        /// the primary action(s)
        /// </summary>
        IList<Frame> Middleware { get; }

        /// <summary>
        /// Frames that would be initially placed behind the primary
        /// action(s)
        /// </summary>
        IList<Frame> Postprocessors { get; }

        /// <summary>
        /// A description of this frame
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Used internally by Jasper for "outbox" mechanics
        /// </summary>
        /// <returns></returns>
        bool ShouldFlushOutgoingMessages();

        MethodCall[] HandlerCalls();
    }
    // ENDSAMPLE


    public static class ChainExtensions
    {

        /// <summary>
        /// Find all of the service dependencies of the current chain
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<Type> ServiceDependencies(this IChain chain, IContainer container)
        {
            return serviceDependencies(chain, container).Distinct();
        }

        private static IEnumerable<Type> serviceDependencies(IChain chain, IContainer container)
        {
            foreach (var handlerCall in chain.HandlerCalls())
            {
                yield return handlerCall.HandlerType;

                foreach (var parameter in handlerCall.Method.GetParameters())
                {
                    yield return parameter.ParameterType;
                }

                var @default = container.Model.For(handlerCall.HandlerType).Default;
                foreach (var dependency in @default.Instance.Dependencies)
                {
                    yield return dependency.ServiceType;
                }
            }
        }
    }
}
