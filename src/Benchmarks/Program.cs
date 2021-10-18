using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TestMessages;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            var json = JsonConvert.SerializeObject(targets);
            File.WriteAllText("targets.json", json);
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
