using System;

namespace Jasper.Diagnostics.Messages
{
    public class MessageTypeModel
    {
        public MessageTypeModel(Type type)
        {
            Name = type?.Name;
            FullName = type?.FullName;
        }

        public string Name { get; }
        public string FullName { get; }
    }
}