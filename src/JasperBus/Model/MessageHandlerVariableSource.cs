using System;
using Jasper.Codegen;

namespace JasperBus.Model
{
    public class MessageHandlerVariableSource : IVariableSource
    {
        public static readonly MessageHandlerVariableSource Instance = new MessageHandlerVariableSource();

        public bool Matches(Type type)
        {
            throw new NotImplementedException();
        }

        public Variable Create(Type type)
        {
            throw new NotImplementedException();
        }
    }
}