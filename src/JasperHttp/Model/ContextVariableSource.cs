using System;
using Jasper.Codegen;

namespace JasperHttp.Model
{
    public class ContextVariableSource : IVariableSource
    {
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