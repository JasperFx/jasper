using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_EnvironmentChecks_IEnvironmentRecorder_environmentRecorder
    public class Jasper_EnvironmentChecks_IEnvironmentRecorder_environmentRecorder : Lamar.IoC.Resolvers.TransientResolver<Jasper.EnvironmentChecks.IEnvironmentRecorder>
    {


        public override Jasper.EnvironmentChecks.IEnvironmentRecorder Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.EnvironmentChecks.EnvironmentRecorder();
        }

    }

    // END: Jasper_EnvironmentChecks_IEnvironmentRecorder_environmentRecorder
    
    
}

