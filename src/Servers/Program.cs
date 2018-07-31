using System;
using System.Threading.Tasks;
using Baseline;
using Oakton;
using Servers.Commands;
using Servers.Docker;

namespace Servers
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return CommandExecutor.For(x =>
            {
                x.RegisterCommands(typeof(Program).Assembly);
                x.DefaultCommand = typeof(StartCommand);
            }).ExecuteAsync(args);
        }


    }
}
