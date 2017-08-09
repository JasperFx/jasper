using System;
using Jasper;
using Microsoft.AspNetCore.Hosting;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            JasperAgent.Run<ConsoleAppRegistry>();
        }
    }

    public class ConsoleAppRegistry : JasperRegistry
    {
        public ConsoleAppRegistry()
        {
            Http.UseKestrel().UseUrls("http://localhost:3001");

            Channels.ListenForMessagesFrom("jasper://localhost:2222/incoming");
        }
    }

    public class HomeEndpoint
    {
        public string Get()
        {
            return "Hello, world.";
        }
    }
}
