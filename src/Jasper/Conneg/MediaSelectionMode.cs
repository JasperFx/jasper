namespace Jasper.Conneg
{
    public enum MediaSelectionMode
    {
        /// <summary>
        /// Allow the acceptance of non-versioned messages, i.e., "application/json"
        /// </summary>
        All,

        /// <summary>
        /// Only accept versioned message content types
        /// </summary>
        VersionedOnly
    }
}