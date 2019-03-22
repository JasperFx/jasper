using System;

namespace Jasper.Http
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class HttpRouteAttribute : Attribute
    {
        public string Method { get; }
        public string RoutePattern { get; }

        protected HttpRouteAttribute(string method, string routePattern)
        {
            Method = method;
            RoutePattern = routePattern;
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a GET HTTP request with the specified route pattern
    /// </summary>
    public class JasperGetAttribute : HttpRouteAttribute
    {
        public JasperGetAttribute(string routePattern) : base("GET", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a HEAD HTTP request with the specified route pattern
    /// </summary>
    public class JasperHeadAttribute : HttpRouteAttribute
    {
        public JasperHeadAttribute(string routePattern) : base("HEAD", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a POST HTTP request with the specified route pattern
    /// </summary>
    public class JasperPostAttribute : HttpRouteAttribute
    {
        public JasperPostAttribute(string routePattern) : base("POST", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a PUT HTTP request with the specified route pattern
    /// </summary>
    public class JasperPutAttribute : HttpRouteAttribute
    {
        public JasperPutAttribute(string routePattern) : base("PUT", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a DELETE HTTP request with the specified route pattern
    /// </summary>
    public class JasperDeleteAttribute : HttpRouteAttribute
    {
        public JasperDeleteAttribute(string routePattern) : base("DELETE", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a OPTIONS HTTP request with the specified route pattern
    /// </summary>
    public class JasperOptionsAttribute : HttpRouteAttribute
    {
        public JasperOptionsAttribute(string routePattern) : base("OPTIONS", routePattern)
        {
        }
    }

    /// <summary>
    /// Signals to Jasper that this method handles a PATCH HTTP request with the specified route pattern
    /// </summary>
    public class JasperPatchAttribute : HttpRouteAttribute
    {
        public JasperPatchAttribute(string routePattern) : base("PATCH", routePattern)
        {
        }
    }
}
