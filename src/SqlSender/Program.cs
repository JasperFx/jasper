using System;
using Jasper.CommandLine;

namespace SqlSender
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<SenderApp>(args);
        }
    }
}
