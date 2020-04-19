using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Jasper.Serialization;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Jasper.Http.Model
{
    public class RouteChain : Chain<RouteChain, ModifyRouteAttribute>
    {
        public static readonly Variable[] HttpContextVariables =
            Variable.VariablesForProperties<HttpContext>(RouteGraph.Context);

        private GeneratedType _generatedType;


        private bool _hasAppliedConfigureAndAttributes;

        private bool _hasDeterminedFrames;

        public RouteChain(MethodCall action, JasperRoute route)
        {
            Action = action;
            Route = route;
            TypeName =
                $"{Action.HandlerType.FullNameInCode().Replace(".", "_")}_{action.Method.Name}_{action.Method.GetParameters().Select(x => x.Name).Join("_")}";

            InputType = route.InputType;
            ResourceType = action.ReturnVariable?.VariableType;

        }

        public RouteChain(MethodCall action) : this(action, JasperRoute.Build(action.HandlerType, action.Method))
        {
        }

        public override string Description => $"JasperRoute '{Route.Description}'";

        public string SourceCode => _generatedType.SourceCode;

        public WriterCollection<IResponseWriter> ConnegWriter { get; set; }

        public ReaderCollection<IRequestReader> ConnegReader { get; set; }

        public IRequestReader Reader { get; set; }
        public IResponseWriter Writer { get; set; }

        public string TypeName { get; }

        /// <summary>
        ///     The .Net method that implements this route
        /// </summary>
        public MethodCall Action { get; }

        /// <summary>
        ///     Information about the HTTP route pattern for this RouteChain
        /// </summary>
        public JasperRoute Route { get; }

        /// <summary>
        ///     May be null
        /// </summary>
        public Type InputType { get; }

        /// <summary>
        ///     May be null
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

        public override MethodCall[] HandlerCalls()
        {
            return new[] {Action};
        }

        public override string ToString()
        {
            return $"{Route.HttpMethod}: {Route.Pattern}";
        }

        public void AssemblyType(GeneratedAssembly generatedAssembly, ConnegRules rules,
            GenerationRules codeRules, IContainer container)
        {
            _generatedType = generatedAssembly.AddType(TypeName, typeof(RouteHandler));
            var handleMethod = _generatedType.MethodFor(nameof(RouteHandler.Handle));

            handleMethod.Frames.AddRange(DetermineFrames(rules, codeRules, container));

            handleMethod.Sources.Add(new ContextVariableSource());
            handleMethod.DerivedVariables.AddRange(HttpContextVariables);
        }

        public List<Frame> DetermineFrames(ConnegRules rules, GenerationRules codeRules, IContainer container)
        {
            if (_hasDeterminedFrames) throw new InvalidOperationException("Has already determined frames");

            _hasDeterminedFrames = true;

            if (!_hasAppliedConfigureAndAttributes)
            {
                rules.Apply(this);
                _hasAppliedConfigureAndAttributes = true;
                applyAttributesAndConfigureMethods(codeRules, container);
            }

            var list = Middleware.ToList();

            var arguments = Route.Segments.OfType<IRoutingFrameSource>();
            foreach (var argument in arguments)
            {
                if (Action.Method.Name == "Put" && Action.HandlerType.Name == "ValuesController")
                {
                    Debug.Assert(true);
                }

                var frame = argument.ToParsingFrame(Action);
                list.Add(frame);
            }


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
            connectHandler(handler);

            return handler;
        }

        private void connectHandler(RouteHandler handler)
        {
            handler.Chain = this;
            Route.Handler = handler;

            handler.Reader = Reader;
            handler.Writer = Writer;
            handler.ConnegReader = ConnegReader;
            handler.ConnegWriter = ConnegWriter;
        }


        public override bool ShouldFlushOutgoingMessages()
        {
            return Action.Method.GetParameters()
                .Any(x => x.ParameterType == typeof(IMessageContext));
        }

        public void AttachPreBuiltHandler(GenerationRules rules, IContainer container, Type[] handlerTypes)
        {
            var fullName = $"{rules.ApplicationNamespace}.{TypeName}";
            var handlerType = handlerTypes.FirstOrDefault(x => x.FullName == fullName);

            if (handlerType == null) return;

            var handler = (RouteHandler) container.QuickBuild(handlerType);
            connectHandler(handler);
        }

        public RouteEndpoint BuildEndpoint(IContainer container)
        {
            var handler = CreateHandler(container);
            var pattern = Route.BuildRoutePattern();

            // TODO -- do more to pick up attributes here like [Authorize] and [AllowAnonymous]

            var metadata = new EndpointMetadataCollection(new HttpMethodMetadata(new string[]{Route.HttpMethod}));

            var endpoint = new RouteEndpoint(c => handler.Handle(c), pattern, Route.Order, metadata, Route.Description);

            return endpoint;
        }
    }
}
