using System;
using System.Reflection;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class UseReader : MethodCall
    {
        private static MethodInfo selectMethod(Type inputType)
        {
            return typeof(IMediaReader)
                .GetMethod(nameof(IMediaReader.ReadFromRequest))
                .MakeGenericMethod(inputType);

        }

        public UseReader(RouteChain chain) : base(typeof(IMediaReader), selectMethod(chain.InputType))
        {
            Target = new Variable(typeof(IMediaReader), nameof(RouteHandler.Reader));

            creates.Add(ReturnVariable);
        }
    }
}