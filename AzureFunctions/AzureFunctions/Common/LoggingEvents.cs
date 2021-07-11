namespace AzureFunctions.Common
{
    /// <summary>
    /// Defines the event ids for logging
    /// </summary>
    public class LoggingEvents
    {
        /// <summary>
        /// The get item
        /// </summary>
        public const int GetItem = 1000;
        /// <summary>
        /// The insert item
        /// </summary>
        public const int InsertItem = 1001;
        /// <summary>
        /// The update item
        /// </summary>
        public const int UpdateItem = 1002;
        /// <summary>
        /// The get item list
        /// </summary>
        public const int GetItemList = 1005;
        /// <summary>
        /// The get item not found
        /// </summary>
        public const int GetItemNotFound = 4000;
        /// <summary>
        /// The container item not found
        /// </summary>
        public const int ContainerNotFound = 4003;
        /// <summary>
        /// The internal error
        /// </summary>
        public const int InternalError = 5000;
        /// <summary>
        /// The upload error
        /// </summary>
        public const int UploadItemError = 5001;
        /// <summary>
        /// The update error
        /// </summary>
        public const int UpdateItemError = 5002;
        /// <summary>
        /// The get item error
        /// </summary>
        public const int GetItemError = 5004;

    }
}
