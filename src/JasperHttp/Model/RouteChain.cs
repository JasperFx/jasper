using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Messaging;
using JasperHttp.ContentHandling;
using JasperHttp.Routing;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Model
{
    public class RouteChain : Chain<RouteChain, ModifyRouteAttribute>
    {
        public static readonly Variable[] HttpContextVariables =
            Variable.VariablesForProperties<HttpContext>(RouteGraph.Context);

        private GeneratedType _generatedType;


        private bool _hasAppliedConfigureAndAttributes;

        public RouteChain(MethodCall action, Route route)
        {
            Action = action;
            Route = route;
            TypeName = $"{Action.HandlerType.FullNameInCode().Replace(".", "_")}_{action.Method.Name}_{action.Method.GetParameters().Select(x => x.Name).Join("_")}";

            InputType = RouteBuilder.DetermineInputType(action.Method);
            ResourceType = action.ReturnVariable?.VariableType;

            Route.Chain = this;
        }

        public RouteChain(MethodCall action) : this(action, RouteBuilder.Build(action.HandlerType, action.Method))
        {

        }

        public override string Description => $"Route '{Route.Description}'";

        public string SourceCode => _generatedType.SourceCode;

        public WriterCollection<IResponseWriter> ConnegWriter { get; set; }

        public ReaderCollection<IRequestReader> ConnegReader { get; set; }

        public IRequestReader Reader { get; set; }
        public IResponseWriter Writer { get; set; }

        public string TypeName { get; }

        /// <summary>
        /// The .Net method that implements this route
        /// </summary>
        public MethodCall Action { get; }

        /// <summary>
        /// Information about the HTTP route pattern for this RouteChain
        /// </summary>
        public Route Route { get; }

        /// <summary>
        /// May be null
        /// </summary>
        public Type InputType { get; }

        /// <summary>
        /// May be null
        /// </summary>
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

        public override string ToString()
        {
            return $"{Route.HttpMethod}: {Route.Pattern}";
        }

        public void AssemblyType(GeneratedAssembly generatedAssembly, ConnegRules rules,
            JasperGenerationRules codeRules)
        {
            _generatedType = generatedAssembly.AddType(TypeName, typeof(RouteHandler));
            var handleMethod = _generatedType.MethodFor(nameof(RouteHandler.Handle));

            handleMethod.Frames.AddRange(DetermineFrames(rules, codeRules));

            handleMethod.Sources.Add(new ContextVariableSource());
            handleMethod.DerivedVariables.AddRange(HttpContextVariables);
        }

        private bool _hasDeterminedFrames = false;

        public List<Frame> DetermineFrames(ConnegRules rules, JasperGenerationRules codeRules)
        {
            if (_hasDeterminedFrames) throw new InvalidOperationException("Has already determined frames");

            _hasDeterminedFrames = true;

            if (!_hasAppliedConfigureAndAttributes)
            {
                rules.Apply(this);
                _hasAppliedConfigureAndAttributes = true;
                applyAttributesAndConfigureMethods(codeRules);
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
            Route.Handler = handler;

            handler.Reader = Reader;
            handler.Writer = Writer;
            handler.ConnegReader = ConnegReader;
            handler.ConnegWriter = ConnegWriter;

            return handler;
        }

        public void WriteRouteMatchMethod(GeneratedType generatedType)
        {
            if (Route.Segments.OfType<RouteArgument>().Any())
            {
                var method = new GeneratedMethod($"Matches{Route.VariableName}", typeof(bool), new Argument(typeof(string[]), "segments"));
                generatedType.AddMethod(method);

                method.Frames.Add(new RouteMatchFrame(Route));
            }


        }

        public override bool ShouldFlushOutgoingMessages()
        {
            return Action.Method.GetParameters()
                .Any(x => x.ParameterType == typeof(IMessageContext));
        }
    }
}
