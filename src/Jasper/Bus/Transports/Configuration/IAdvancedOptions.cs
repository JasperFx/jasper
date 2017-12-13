using Jasper.Conneg;

namespace Jasper.Bus.Transports.Configuration
{
    public interface IAdvancedOptions
    {
        /// <summary>
        /// Default is true. Fail during bootstrapping if there are any
        /// detected validation errors in message subscriptions or environment
        /// tests
        /// </summary>
        bool ThrowOnValidationErrors { get; set; }

        /// <summary>
        /// Force the media selection to be stricter so
        /// </summary>
        MediaSelectionMode MediaSelectionMode { get; set; }

        /// <summary>
        /// Disables all messaging transport bootstrapping
        /// </summary>
        bool DisableAllTransports { get; set; }
    }
}
