using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Http.Routing;
using Jasper.Internals.Codegen;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace Jasper.Http.Model
{
    public class RouteChain : Chain<RouteChain, ModifyRouteAttribute>,IGenerates<RouteHandler>
    {
        public static readonly Variable[] HttpContextVariables = Variable.VariablesForProperties<HttpContext>(RouteGraph.Context);
        private Type _readerType;
        private Type _writerType;

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
            {
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Cannot find method named '{methodName}' in type {handlerType.FullName}");
            }

            var call = new MethodCall(handlerType, method);

            return new RouteChain(call);
        }



        public RouteChain(MethodCall action)
        {
            Action = action;
            Route = RouteBuilder.Build(action.HandlerType, action.Method);
            TypeName = $"{Action.HandlerType.FullName.Replace(".", "_")}_{action.Method.Name}";

            InputType = RouteBuilder.DetermineInputType(action.Method);
            ResourceType = action.ReturnVariable?.VariableType;
        }

        protected override MethodCall[] handlerCalls()
        {
            return new[] {Action};
        }

        public string SourceCode { get; set; }

        public RouteHandler Create(Type[] types, Func<Type, object> builder)
        {
            var type = types.FirstOrDefault(x => x.Name == TypeName);
            if (type == null)
            {
                throw new ArgumentOutOfRangeException(nameof(types), $"Could not find a type named '{TypeName}' in this assembly");
            }

            var handler = builder(type).As<RouteHandler>();

            handler.Chain = this;

            handler.Reader = Reader;
            handler.Writer = Writer;
            handler.ConnegReader = ConnegReader;
            handler.ConnegWriter = ConnegWriter;

            return handler;
        }

        public void ApplyConneg(SerializationGraph graph)
        {
            if (InputType != null)
            {
                ConnegReader = graph.ReaderFor(InputType);
            }

            if (ResourceType != null)
            {
                ConnegWriter = graph.WriterFor(ResourceType);
            }
        }

        public ModelWriter ConnegWriter { get; set; }

        public ModelReader ConnegReader { get; set; }

        public IMessageDeserializer Reader { get; set; }
        public IMessageSerializer Writer { get; set; }

        public string TypeName { get; }

        public MethodCall Action { get; }

        public Route Route { get; }
        public Type InputType { get; }
        public Type ResourceType { get; }

        public override string ToString()
        {
            return $"{Route.HttpMethod}: {Route.Pattern}";
        }

        public GeneratedClass ToClass(GenerationRules rules)
        {
            try
            {
                var @class = new GeneratedClass(rules, TypeName)
                {
                    BaseType = typeof(RouteHandler)
                };

                var frames = DetermineFrames();
                var method = new GeneratedMethod(nameof(RouteHandler.Handle),
                    new Argument[] {Argument.For<HttpContext>(RouteGraph.Context)}, frames)
                {
                    Overrides = true
                };

                method.Sources.Add(new ContextVariableSource());
                method.DerivedVariables.AddRange(HttpContextVariables);

                @class.AddMethod(method);

                return @class;
            }
            catch (Exception e)
            {
                throw new CodeGenerationException(this, e);
            }
        }

        public List<Frame> DetermineFrames()
        {
            applyAttributesAndConfigureMethods();
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
    }
}
