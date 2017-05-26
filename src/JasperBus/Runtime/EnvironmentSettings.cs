using System;

namespace JasperBus.Runtime
{
    public class EnvironmentSettings
    {
        private string _machineName;

        public string MachineName
        {
            get
            {
                if (_machineName == null)
                    return _machineName = Environment.MachineName;
                return _machineName;
            }
            set { _machineName = value; }
        }
    }
}
