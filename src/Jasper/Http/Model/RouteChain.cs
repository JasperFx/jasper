using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Lamar;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Model
{
    public class RouteChain : Chain<RouteChain, ModifyRouteAttribute>
    {
        public static readonly Variable[] HttpContextVariables =
            Variable.VariablesForProperties<HttpContext>(RouteGraph.Context);

        private GeneratedType _generatedType;

        public RouteChain(MethodCall action)
        {
            Action = action;
            Route = RouteBuilder.Build(action.HandlerType, action.Method);
            TypeName = $"{Action.HandlerType.FullName.Replace(".", "_")}_{action.Method.Name}";

            InputType = RouteBuilder.DetermineInputType(action.Method);
            ResourceType = action.ReturnVariable?.VariableType;

            Route.Chain = this;
        }

        public RouteChain(MethodCall action, string url)
        {
            Action = action;
            Route = RouteBuilder.Build(url, action.HandlerType, action.Method);
            TypeName = $"{Action.HandlerType.FullName.Replace(".", "_")}_{action.Method.Name}";

            InputType = RouteBuilder.DetermineInputType(action.Method);
            ResourceType = action.ReturnVariable?.VariableType;

            Route.Chain = this;
        }

        public string SourceCode => _generatedType.SourceCode;

        public ModelWriter ConnegWriter { get; set; }

        public ModelReader ConnegReader { get; set; }

        public IMessageDeserializer Reader { get; set; }
        public IMessageSerializer Writer { get; set; }

        public string TypeName { get; }

        public MethodCall Action { get; }

        public Route Route { get; }
        public Type InputType { get; }
        public Type ResourceType { get; }

        public static RouteChain For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            var call = new MethodCall(typeof(T), method);

            return new RouteChain(call);
        }

        public static RouteChain For<T>(string methodName)
        {
            var handlerType = typeof(T);
            var method = handlerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            if (method == null)
                throw new ArgumentOutOfRangeException(nameof(methodName),
                    $"Cannot find method named '{methodName}' in type {handlerType.FullName}");

            var call = new MethodCall(handlerType, method);

            return new RouteChain(call);
        }

        protected override MethodCall[] handlerCalls()
        {
            return new[] {Action};
        }


        public void ApplyConneg(SerializationGraph graph)
        {
            if (InputType != null) ConnegReader = graph.ReaderFor(InputType);

            if (ResourceType != null) ConnegWriter = graph.WriterFor(ResourceType);
        }

        public override string ToString()
        {
            return $"{Route.HttpMethod}: {Route.Pattern}";
        }

        public void AssemblyType(GeneratedAssembly generatedAssembly, ConnegRules rules)
        {
            _generatedType = generatedAssembly.AddType(TypeName, typeof(RouteHandler));
            var handleMethod = _generatedType.MethodFor(nameof(RouteHandler.Handle));

            handleMethod.Frames.AddRange(DetermineFrames(rules));

            handleMethod.Sources.Add(new ContextVariableSource());
            handleMethod.DerivedVariables.AddRange(HttpContextVariables);
        }


        private bool _hasAppliedConfigureAndAttributes;
        public List<Frame> DetermineFrames(ConnegRules rules)
        {
            if (!_hasAppliedConfigureAndAttributes)
            {
                rules.Apply(this);
                _hasAppliedConfigureAndAttributes = true;
                applyAttributesAndConfigureMethods();
            }

            var list = Middleware.ToList();

            list.AddRange(Route.Segments.OfType<IRoutingFrameSource>().Select(x => x.ToParsingFrame(Action)));


            list.Add(Action);

            list.AddRange(Postprocessors);

            return list;
        }

        public bool RespondsToMethod(string httpMethod)
        {
            return Route.HttpMethod.EqualsIgnoreCase(httpMethod);
        }

        public RouteHandler CreateHandler(IContainer container)
        {
            var handler = container.QuickBuild(_generatedType.CompiledType).As<RouteHandler>();
            handler.Chain = this;

            handler.Reader = Reader;
            handler.Writer = Writer;
            handler.ConnegReader = ConnegReader;
            handler.ConnegWriter = ConnegWriter;

            return handler;
        }
    }
}
