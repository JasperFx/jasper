﻿using System;
using Lamar;

namespace Jasper.Configuration
{
    /// <summary>
    ///     Tells Jasper to ignore this assembly in its determination of the application assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class JasperFeatureAttribute : IgnoreAssemblyAttribute
    {
    }
}
