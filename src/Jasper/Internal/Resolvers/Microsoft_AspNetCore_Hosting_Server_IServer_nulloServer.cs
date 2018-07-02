using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Microsoft_AspNetCore_Hosting_Server_IServer_nulloServer
    public class Microsoft_AspNetCore_Hosting_Server_IServer_nulloServer : Lamar.IoC.Resolvers.TransientResolver<Microsoft.AspNetCore.Hosting.Server.IServer>
    {


        public override Microsoft.AspNetCore.Hosting.Server.IServer Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.Http.NulloServer();
        }

    }

    // END: Microsoft_AspNetCore_Hosting_Server_IServer_nulloServer
    
    
}

