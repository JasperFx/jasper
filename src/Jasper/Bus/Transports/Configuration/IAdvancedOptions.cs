using Jasper.Conneg;

namespace Jasper.Bus.Transports.Configuration
{
    public interface IAdvancedOptions
    {
        bool ThrowOnValidationErrors { get; set; }
        MediaSelectionMode MediaSelectionMode { get; set; }
        NoRouteBehavior NoMessageRouteBehavior { get; set; }
        bool DisableAllTransports { get; set; }
    }
}
