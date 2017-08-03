using Alba;
using Jasper;
using Microsoft.AspNetCore.Hosting;

namespace AlbaForJasper
{
    public class JasperAlbaUsage : SystemUnderTestBase
    {
        private readonly JasperRuntime _runtime;

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
