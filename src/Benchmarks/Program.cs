using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PersistenceRunner>();
        }
    }

    public static class TargetHandler
    {
        public static void Handle(Target target)
        {
            var data = target.Children.Sum(x => x.Number);
        }
    }
}
