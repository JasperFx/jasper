using System;

namespace Jasper.Messaging
{
    /// <summary>
    /// Mark this representation of a message as a named version
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class VersionAttribute : Attribute
    {
        public string Version { get; }

        public VersionAttribute(string version)
        {
            Version = version;
        }


    }
}
