using AzureFunctions.Models;

namespace AzureFunctions.DTO
{
    public class JobStatusDescription
    {

        /// <summary>
        /// Converts an jobStatus number inside an encoded jobStatus description, to the standard jobStatus response
        /// </summary>
        /// <param name="encodedStatusDescription">The jobStatus description</param>
        /// <returns>The decoded job status message and number</returns>
        public static string GetjobStatusMessage(JobStatus jobStatus)
        {

            int jobStatusNumber = (int)jobStatus;
            // list of job status jobStatuss
            switch (jobStatusNumber)
            {
                case 1:
                    {
                        return "Image Obtained";
                    }
                case 2:
                    {
                        return "Image Being Converted";
                    }
                case 3:
                    {
                        return "Image Converted with success";
                    }
                case 4:
                    {
                        return "Image Failed Conversion";
                    }
                case 5:
                    {
                        return "Image Converted with success, ImageSource Deleted";
                    }
                default:
                    {
                        return $"Raw jobStatus: ${jobStatusNumber}";
                    }

            }
        }

    }
}
