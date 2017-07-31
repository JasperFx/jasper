using System;
using System.Threading.Tasks;
using Alba;
using Jasper;

namespace AlbaForJasper
{
    public static class JasperRuntimeAlbaExtensions
    {
        /// <summary>
        /// Run an Alba scenario test against a Jasper application
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Task<IScenarioResult> Scenario(this JasperRuntime runtime, Action<Scenario> configuration)
        {
            var system = new JasperAlbaUsage(runtime);
            return system.Scenario(configuration);
        }
    }
}