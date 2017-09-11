namespace Jasper.Bus.Transports.Configuration
{
    public interface IAdvancedOptions
    {
        bool ThrowOnValidationErrors { get; set; }
        bool AllowNonVersionedSerialization { get; set; }
        NoRouteBehavior NoMessageRouteBehavior { get; set; }
    }
}
