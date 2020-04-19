using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Jasper.Http.Routing
{
    public interface IUrlRegistry
    {
        /// <summary>
        ///     Look up the Url that would accept the type of the model object
        ///     as its input type
        /// </summary>
        /// <param name="model"></param>
        /// <param name="httpMethod">
        ///     Specify the HTTP method if there may be multiple routes that accepts this model type to
        ///     disambiguate
        /// </param>
        /// <returns></returns>
        string UrlFor(object model, string httpMethod = null);

        /// <summary>
        ///     Look up the Url that would accept the type T as its request body
        /// </summary>
        /// <param name="httpMethod">
        ///     Specify the HTTP method if there may be multiple routes that accepts this model type to
        ///     disambiguate
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string UrlForType<T>(string httpMethod = null);

        /// <summary>
        ///     Look up the Url for the specified handler type and method name
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="method"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        string UrlFor(Type handlerType, MethodInfo method = null, string httpMethod = null);

        /// <summary>
        ///     Look up the Url for the specified method. This will fill in route arguments to
        ///     the url
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        string UrlFor<THandler>(Expression<Action<THandler>> expression);

        /// <summary>
        ///     Find the Url for a named route
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        string UrlFor(string routeName, IDictionary<string, object> parameters = null);

        /// <summary>
        ///     Find the Url for this handler type with the specified method name
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        string UrlFor(Type handlerType, string methodName);
    }
}
