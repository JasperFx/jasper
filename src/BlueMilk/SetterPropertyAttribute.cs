using System;

namespace BlueMilk
{
    /// <summary>
    /// Marks a Property in a Pluggable class as filled by setter injection 
    /// </summary>
    [Obsolete("Not sure yet if we'll support this later")]
    [AttributeUsage(AttributeTargets.Property)]
    public class SetterPropertyAttribute : Attribute
    {
    }
}