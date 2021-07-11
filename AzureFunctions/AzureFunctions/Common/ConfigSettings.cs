namespace AzureFunctions.Common
{
    /// <summary>
    ///  Configuration Variables
    /// </summary>
    public static class ConfigSettings
    {
        /// <summary>
        ///  GrayScale container 
        /// </summary>
        public const string GRAYSCALEIMAGES_CONTAINERNAME = "converttogreyscale";
        /// <summary>
        /// Sepia Container
        /// </summary>
        public const string SEPIAIMAGES_CONTAINERNAME = "converttosepia";

        /// <summary>
        /// convertedimages container
        /// </summary>
        public const string CONVERTED_IMAGES_CONTAINERNAME = "convertedimages";
        /// <summary>
        /// failedimages
        /// </summary>
        public const string FAILED_IMAGES_CONTAINERNAME = "failedimages";
        /// <summary>
        /// AzureWebJobsStorage string conecction name
        /// </summary>
        public const string STORAGE_CONNECTIONSTRING_NAME = "AzureWebJobsStorage";
        /// <summary>
        /// imageconversionjobs table name 
        /// </summary>
        public const string JOBS_TABLENAME = "imageconversionjobs";
        /// <summary>
        ///  imageconversions table partiion key
        /// </summary>
        public const string IMAGEJOBS_PARTITIONKEY = "imageconversions";
        /// <summary>
        /// JobId image id 
        /// </summary>
        public const string JOBID_METADATA_NAME = "JobId";

    }
}
