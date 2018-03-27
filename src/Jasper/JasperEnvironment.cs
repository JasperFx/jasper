using System;
using Microsoft.Extensions.Configuration;

namespace Jasper
{
    /// <summary>
    ///     Strictly used to override the ASP.Net Core environment name on bootstrapping
    /// </summary>
    [Obsolete("Won't be necessary after GH-339")]
    public static class JasperEnvironment
    {
        public static string Name { get; set; }

    }
}
