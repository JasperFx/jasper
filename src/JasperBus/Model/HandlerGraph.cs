using System;
using System.Collections.Generic;
using Jasper.Codegen;
using JasperBus.Runtime.Invocation;
using StructureMap;

namespace JasperBus.Model
{
    public class HandlerGraph : HandlerSet<HandlerChain, IInvocationContext, MessageHandler>
    {
        public readonly List<MethodCall> HandlerMethods = new List<MethodCall>();

        public HandlerGraph(GenerationConfig generation) : base(generation, "context")
        {
            
        }

        public MessageHandler HandlerFor(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Compile(IContainer container)
        {
            // build the all the chains
        }
    }
}