using System;

namespace BlueMilk
{
    /// <summary>
    ///     Used to override the constructor of a class to be used by BlueMilk to create
    ///     the concrete type
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DefaultConstructorAttribute : Attribute
    {
    }
}