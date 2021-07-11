using AzureFunctions.Common;
using AzureFunctions.DTO;
using AzureFunctions.Models;
using AzureFunctions.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public static class ImageStatusUpdaterFailed
    {

        const string ImageStatusUpdaterFailedRoute = ConfigSettings.FAILED_IMAGES_CONTAINERNAME + "/{name}";
        /// <summary>
        /// Updates the job table with fails indication for every file that was not converted and stored in the
        /// failedimages container
        /// </summary>
        /// <param name="blockBlob">The block BLOB.</param>
        /// <param name="log">The log.</param>
        [FunctionName("ImageStatusUpdaterFailed")]
        public static async Task Run([BlobTrigger(ImageStatusUpdaterFailedRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob blockBlob, ILogger log)
        {
            // Retrieve the job id
            await blockBlob.FetchAttributesAsync();
            if (blockBlob.Metadata.ContainsKey(ConfigSettings.JOBID_METADATA_NAME))
            {
                string jobId = blockBlob.Metadata[ConfigSettings.JOBID_METADATA_NAME];
                string imageResult = blockBlob.Uri.AbsoluteUri;
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{blockBlob.Name} \n JobId: [{jobId}]");
                JobStatus status = JobStatus.ImageConvertedFailed;
                await ImageConvertionService.UpdateJobTableWithStatus(log, jobId, status: status, message: JobStatusDescription.GetjobStatusMessage(status), imageResult);
            }
            else
            {
                log.LogError($"The blob {blockBlob.Name} is missing its {ConfigSettings.JOBID_METADATA_NAME} metadata can't update the job");
            }
        }
    }
}
