using Alba;
using Jasper;
using Microsoft.AspNetCore.Hosting;

namespace AlbaForJasper
{
    public class JasperAlbaUsage : SystemUnderTestBase
    {
        private readonly JasperRuntime _runtime;

        // TODO -- bring in the IHostingEnvironment attached to the runtime
        // When it exists. See https://github.com/JasperFx/jasper/issues/91
        public JasperAlbaUsage(JasperRuntime runtime) : base(null)
        {
            _runtime = runtime;
        }


        protected override IWebHost buildHost()
        {
            return _runtime.Get<IWebHost>();
        }
    }
}