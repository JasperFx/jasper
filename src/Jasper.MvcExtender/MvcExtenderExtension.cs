using Baseline;
using Jasper;
using Jasper.Configuration;
using Jasper.MvcExtender;
using Microsoft.AspNetCore.Mvc;

[assembly:JasperModule(typeof(MvcExtenderExtension))]

namespace Jasper.MvcExtender
{
    public class MvcExtenderExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.HttpRoutes.IncludeTypes(x => x.CanBeCastTo<ControllerBase>());
        }
    }
}
