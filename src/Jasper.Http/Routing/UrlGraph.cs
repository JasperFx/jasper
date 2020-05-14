﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using HandlerMethods = Baseline.LightweightCache<string, Jasper.Http.Routing.JasperRoute>;

namespace Jasper.Http.Routing
{
    public class UrlGraph : IUrlRegistry
    {
        private readonly LightweightCache<Type, List<JasperRoute>> _routesByInputModel
            = new LightweightCache<Type, List<JasperRoute>>(_ => new List<JasperRoute>());

        private readonly LightweightCache<Type, HandlerMethods> _routesPerHandler
            = new LightweightCache<Type, HandlerMethods>(type => new HandlerMethods());

        private readonly LightweightCache<string, JasperRoute> _routesPerName
            = new LightweightCache<string, JasperRoute>();

        public string UrlFor(object model, string httpMethod = null)
        {
            var route = RouteFor(model, httpMethod);
            assertNoParameters(route);

            return "/" + route.Pattern;
        }

        public string UrlForType<T>(string httpMethod = null)
        {
            var route = RouteFor<T>(httpMethod);

            assertNoParameters(route);

            return "/" + route.Pattern;
        }

        public string UrlFor(Type handlerType, MethodInfo method = null, string httpMethod = null)
        {
            var route = method == null ? RouteFor(handlerType, httpMethod) : RouteFor(handlerType, method);

            assertNoParameters(route);

            return "/" + route.Pattern;
        }

        public string UrlFor<THandler>(Expression<Action<THandler>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            var route = RouteFor(typeof(THandler), method);

            return route.ReadRouteDataFromMethodArguments(expression);
        }

        public string UrlFor(Type handlerType, string methodName)
        {
            var routes = _routesPerHandler[handlerType];
            var route = routes.FirstOrDefault(x => x.Method.Name == methodName);

            if (route == null)
                throw new ArgumentOutOfRangeException(
                    $"Could not find a route for handler {handlerType.FullName}.{methodName}()");

            return "/" + route.Pattern;
        }

        public string UrlFor(string routeName, IDictionary<string, object> parameters = null)
        {
            var route = RouteByName(routeName);
            if (route == null) throw new UrlResolutionException($"There are no routes with the name '{routeName}'");

            if (parameters == null)
            {
                assertNoParameters(route);
                return "/" + route.Pattern;
            }

            return route.ToUrlFromParameters(parameters);
        }


        public void Register(JasperRoute route)
        {
            _routesPerName[route.Name] = route;
            if (route.InputType != null) _routesByInputModel[route.InputType].Add(route);

            if (route.HandlerType != null) _routesPerHandler[route.HandlerType][route.Method.Name] = route;
        }

        public JasperRoute RouteFor(object model, string httpMethod = null)
        {
            var routes = _routesByInputModel[model.GetType()];
            return resolveRoute(httpMethod, routes);
        }

        private static void assertNoParameters(JasperRoute route)
        {
            if (route.HasParameters || route.HasSpread)
                throw new UrlResolutionException($"JasperRoute {route} has arguments and cannot be resolved this way");
        }

        public JasperRoute RouteFor<T>(string httpMethod = null)
        {
            return RouteFor(typeof(T), httpMethod);
        }

        public JasperRoute RouteFor(Type handlerOrInputType, string httpMethod = null)
        {
            var routes = _routesPerHandler[handlerOrInputType].Concat(_routesByInputModel[handlerOrInputType]);
            return resolveRoute(httpMethod, routes);
        }

        private static JasperRoute resolveRoute(string httpMethod, IEnumerable<JasperRoute> routes)
        {
            if (!routes.Any()) throw new UrlResolutionException("There are no matching routes");

            if (routes.Count() == 1)
            {
                var onlyOne = routes.Single();
                if (httpMethod.IsEmpty() || onlyOne.HttpMethod.EqualsIgnoreCase(httpMethod)) return onlyOne;

                throw new UrlResolutionException(
                    $"The matching route ({onlyOne.HttpMethod}:{onlyOne.Pattern}) is a mismatch on the requested Http verb '{httpMethod}'");
            }

            if (httpMethod.IsEmpty())
                throw new UrlResolutionException(
                    $"Multiple matches, try searching with the Http Verb. Found: {routes.Select(x => x.ToString()).Join(", ")}");

            var matching = routes.SingleOrDefault(x => x.HttpMethod.EqualsIgnoreCase(httpMethod));
            if (matching == null)
                throw new UrlResolutionException($"There are no matching routes for Http Verb '{httpMethod}'");

            return matching;
        }

        public JasperRoute RouteFor(Type handlerType, MethodInfo method)
        {
            if (!_routesPerHandler.Has(handlerType))
                throw new UrlResolutionException($"There are no matching routes for handler {handlerType.FullName}");

            var routes = _routesPerHandler[handlerType];
            if (!routes.Has(method.Name))
                throw new UrlResolutionException($"No route matches the method {handlerType.FullName}.{method.Name}()");

            var route = routes[method.Name];

            return route;
        }

        private JasperRoute RouteByName(string routeName)
        {
            return _routesPerName.Has(routeName) ? _routesPerName[routeName] : null;
        }
    }
}
