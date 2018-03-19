using System;
using System.IO;
using System.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace FindMessageUsages
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Environment.CurrentDirectory + "/../Pinger/Pinger.csproj";
            path = Path.GetFullPath(path);


            AnalyzerManager manager = new AnalyzerManager();
            manager.GetProject(path);

            Console.WriteLine("Starting");
            var workspace = manager.GetWorkspace();

            var project = workspace.CurrentSolution.Projects.Single();

            var compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

            var isFound = compilation.ContainsSymbolsWithName(name => name == "Program", SymbolFilter.All);
            Console.WriteLine(isFound);

            foreach (var reference in project.Documents)
            {
                Console.WriteLine(reference.FilePath);
            }

            Console.WriteLine("Go");
        }
    }
}
