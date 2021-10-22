using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using LamarCodeGeneration;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Jasper.Http
{
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
            MethodFilters.Includes += m => m.HasAttribute<HttpMethodAttribute>();

            MethodFilters.Includes += m =>
            {
                return HttpVerbs.All.Contains(m.Name, StringComparer.OrdinalIgnoreCase) ||
                       HttpVerbs.All.Any(x => m.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase));
            };


            IncludeClassesSuffixedWithEndpoint();
        }

        internal UrlGraph Urls { get; } = new UrlGraph();

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


        public void ApplyPolicies(GenerationRules rules)
        {
            foreach (var policy in _policies) policy.Apply(Routes, rules);
        }
    }
}
