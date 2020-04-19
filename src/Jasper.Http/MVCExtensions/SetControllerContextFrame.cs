using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Http.MVCExtensions
{
    public class SetControllerContextFrame : SyncFrame
    {
        private readonly Type _endpointType;
        private Variable _context;
        private Variable _endpoint;

        public SetControllerContextFrame(Type endpointType)
        {
            _endpointType = endpointType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"{_endpoint.Usage}.{nameof(ControllerBase.ControllerContext)} = {_context.Usage};");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _endpoint = chain.FindVariable(_endpointType);
            yield return _endpoint;

            _context = chain.FindVariable(typeof(ControllerContext));
            yield return _context;
        }
    }
}
