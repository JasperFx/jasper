using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace JasperHttp.Model
{
   

    public class RouteChain : IGenerates<RouteHandler>
    {
        public static readonly Variable[] HttpContextVariables = Variable.VariablesForProperties<HttpContext>("input");

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

        public IGenerationModel ToGenerationModel(IGenerationConfig config)
        {
            // TODO -- apply conneg policies here
            // TODO -- look for attributes
            // TODO -- add extra frames
            // TODO -- return the Route generation model
            var frames = new List<Frame>();
            frames.Add(Action);


            return new RouteHandlerGenerationModel(TypeName, config, frames);
        }

        public string SourceCode { get; set; }

        public RouteHandler Create(Type[] types, IContainer container)
        {
            var type = types.FirstOrDefault(x => x.Name == TypeName);
            if (type == null)
            {
                throw new ArgumentOutOfRangeException(nameof(types), $"Could not find a type named '{TypeName}' in this assembly");
            }

            var handler = container.GetInstance(type).As<RouteHandler>();

            handler.Chain = this;

            return handler;
        }

        public string TypeName { get; }

        public MethodCall Action { get; }

        public Route Route { get; }
        public Type InputType { get; }
        public Type ResourceType { get; }

        public override string ToString()
        {
            return $"{Route.HttpMethod}: {Route.Pattern}";
        }

        public GeneratedClass ToClass(IGenerationConfig config)
        {
            var @class = new GeneratedClass(config, TypeName)
            {
                BaseType = typeof(RouteHandler)
            };

            var method = new GeneratedMethod(nameof(RouteHandler.Handle),
                new Argument[] {Argument.For<HttpContext>("input")}, new List<Frame> {Action})
            {
                Overrides = true
            };

            method.DerivedVariables.AddRange(HttpContextVariables);

            @class.AddMethod(method);

            return @class;
        }
    }
}