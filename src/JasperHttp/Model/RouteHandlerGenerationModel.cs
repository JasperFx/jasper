using System.Collections.Generic;
using Jasper.Codegen;

namespace JasperHttp.Model
{
    public class RouteHandlerGenerationModel : GenerationModel<RouteHandler>
    {
        public RouteHandlerGenerationModel(string className, IGenerationConfig config, IList<Frame> frames)
            : base(className, RouteGraph.Context, new ContextVariableSource(), config, frames)
        {
        }
    }
}