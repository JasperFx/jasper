using System;

namespace Jasper.Attributes
{
    /// <summary>
    ///     Base class for an attribute that will customize how
    ///     a message type is sent by Jasper by modifying the Envelope
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ModifyEnvelopeAttribute : Attribute
    {
        public abstract void Modify(Envelope envelope);
    }
}
