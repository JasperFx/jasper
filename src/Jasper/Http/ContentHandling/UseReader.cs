using System;
using System.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class UseReader : MethodCall
    {
        private static MethodInfo selectMethod(Type inputType)
        {
            return typeof(IMessageDeserializer)
                .GetMethod(nameof(IMessageDeserializer.ReadFromRequest))
                .MakeGenericMethod(inputType);

        }

        public UseReader(RouteChain chain, bool isLocal) : base(typeof(IMessageDeserializer), selectMethod(chain.InputType))
        {

            if (isLocal)
            {
                Target = new Variable(typeof(IMessageDeserializer), nameof(RouteHandler.Reader));
            }

            creates.Add(ReturnVariable);
        }
    }
}
