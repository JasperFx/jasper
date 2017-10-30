using System;
using System.IO;

namespace Jasper.EnvironmentChecks
{
    // SAMPLE: FileExistsCheck
    public class FileExistsCheck : IEnvironmentCheck
    {
        private readonly string _file;

        public FileExistsCheck(string file)
        {
            _file = file;
        }

        public void Assert(JasperRuntime runtime)
        {
            if (!File.Exists(_file))
            {
                throw new Exception($"File {_file} cannot be found!");
            }
        }

        public override string ToString()
        {
            return $"File {_file} exists";
        }
    }
    // ENDSAMPLE
}
