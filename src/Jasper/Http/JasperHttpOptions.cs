using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Lamar;

namespace Jasper.Http
{
    public enum ComplianceMode
    {
        /// <summary>
        ///     Use this mode to retain ASP.Net Core's built in scoped container behavior. This is only necessary if you are using
        ///     middleware
        ///     or ActionFilters that use the HttpContext.RequestServices
        /// </summary>
        FullyCompliant,

        /// <summary>
        ///     Faster performance by removing the built in ASP.Net Core scoped container per HTTP request behavior. May
        ///     break some ASP.Net Core middleware and ActionFilter usages
        /// </summary>
        GoFaster
    }

    public partial class JasperHttpOptions
    {
        private readonly IList<IRoutePolicy> _policies = new List<IRoutePolicy>();
        internal readonly RouteGraph Routes = new RouteGraph();

        public JasperHttpOptions()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.Name == "Configure";

            MethodFilters.Excludes += m => m.DeclaringType == typeof(object);
            MethodFilters.Excludes += m => m.HasAttribute<JasperIgnoreAttribute>();
            MethodFilters.Excludes += m => m.DeclaringType.HasAttribute<JasperIgnoreAttribute>();

            MethodFilters.Includes += m => m.Name.EqualsIgnoreCase("Index");
            MethodFilters.Includes += m => m.HasAttribute<HttpRouteAttribute>();

            MethodFilters.Includes += m =>
            {
                return HttpVerbs.All.Contains(m.Name, StringComparer.OrdinalIgnoreCase) ||
                       HttpVerbs.All.Any(x => m.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase));
            };


            IncludeClassesSuffixedWithEndpoint();
        }

        /// <summary>
        ///     Make your Jasper app faster in handling HTTP routes, or more compliant with ASP.Net Core
        /// </summary>
        public ComplianceMode AspNetCoreCompliance { get; set; } = ComplianceMode.FullyCompliant;

        /// <summary>
        ///     Completely enable or disable all Jasper HTTP features
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     After bootstrapping, this would be the compiled router for the HTTP support
        /// </summary>
        public Router Router { get; internal set; }

        public Task IsReady { get; private set; }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void GlobalPolicy<T>() where T : IRoutePolicy, new()
        {
            GlobalPolicy(new T());
        }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        public void GlobalPolicy(IRoutePolicy policy)
        {
            _policies.Add(policy);
        }

        // Call this in UseJasper()
        internal void StartFindingRoutes(Assembly assembly)
        {
            if (!Enabled) return;

            IsReady = FindActions(assembly).ContinueWith(t =>
            {
                var actions = t.Result;
                foreach (var methodCall in actions) Routes.AddRoute(methodCall);
            });
        }

        // Call this from the activator
        internal Task<RouteTree> BuildRouting(IContainer container, JasperGenerationRules generation)
        {
            if (!Enabled) return null;

            return IsReady.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception;
                }

                foreach (var policy in _policies) policy.Apply(Routes, generation);

                Routes.AssertNoDuplicateRoutes();

                var tree = new RouteTree(this, generation);
                tree.CompileAll(container);


                return tree;
            });
        }


        internal void Describe(IJasperHost runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses) writer.WriteLine($"Now listening on: {url}");
        }
    }

    public interface IRoutePatternStrategy
    {
        bool Matches(MethodInfo method);
    }
}
