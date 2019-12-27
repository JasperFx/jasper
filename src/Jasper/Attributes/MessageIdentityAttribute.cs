using System;

namespace Jasper.Attributes
{
    /// <summary>
    ///     Used to override Jasper's default behavior for identifying a message type.
    ///     Useful for integrating with other services without having to share a DTO
    ///     type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageIdentityAttribute : Attribute
    {
        public MessageIdentityAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }

        public int Version { get; set; }

        public string GetName()
        {
            return Version == 0 ? Alias : $"{Alias}.V{Version}";
        }
    }
}
