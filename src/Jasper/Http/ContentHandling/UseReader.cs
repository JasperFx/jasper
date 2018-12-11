using System;
using System.Reflection;
using Jasper.Conneg;
using Jasper.Http.Model;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Http.ContentHandling
{
    public class UseReader : MethodCall
    {
        public UseReader(RouteChain chain, bool isLocal) : base(typeof(IMessageDeserializer),
            selectMethod(chain.InputType))
        {
            if (isLocal) Target = new Variable(typeof(IMessageDeserializer), nameof(RouteHandler.Reader));

            creates.Add(ReturnVariable);
        }

        private static MethodInfo selectMethod(Type inputType)
        {
            return typeof(IMessageDeserializer)
                .GetMethod(nameof(IMessageDeserializer.ReadFromRequest))
                .MakeGenericMethod(inputType);
        }
    }
}
