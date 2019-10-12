using System;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Builder;

namespace JasperHttp
{
    public static class ApplicationBuilderExtensions
    {
        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";

        /// <summary>
        ///     Add Jasper's middleware to the application's RequestDelegate pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationBuilder UseJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
                throw new InvalidOperationException("Jasper has already been applied to this web application");

            return Router.BuildOut(app);
        }

        internal static void MarkJasperHasBeenApplied(this IApplicationBuilder builder)
        {
            if (!builder.Properties.ContainsKey(JasperHasBeenApplied))
                builder.Properties.Add(JasperHasBeenApplied, true);
        }

        internal static bool HasJasperBeenApplied(this IApplicationBuilder builder)
        {
            return builder.Properties.ContainsKey(JasperHasBeenApplied);
        }

    }
}
