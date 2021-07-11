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
    public static class ImageStatusUpdaterSuccess
    {
        const string ImageStatusUpdaterSuccessRoute = ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME + "/{name}";
        /// <summary>
        /// Updates the job table with success indication for every file that was converted and stored in the
        /// convertedimages container
        /// </summary>
        /// <param name="blockBlob">The block BLOB.</param>
        /// <param name="log">The log.</param>
        [FunctionName("ImageStatusUpdaterSuccess")]
        public static async Task Run([BlobTrigger(ImageStatusUpdaterSuccessRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob blockBlob, ILogger log)
        {
            // Retrieve the job id
            await blockBlob.FetchAttributesAsync();
            if (blockBlob.Metadata.ContainsKey(ConfigSettings.JOBID_METADATA_NAME))
            {
                string jobId = blockBlob.Metadata[ConfigSettings.JOBID_METADATA_NAME];
                string imageResult = blockBlob.Uri.AbsoluteUri;
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{blockBlob.Name} \n JobId: [{jobId}]");
                JobStatus status = JobStatus.ImageConvertedSuccess;
                await ImageConvertionService.UpdateJobTableWithStatus(log, jobId, status: status, message: JobStatusDescription.GetjobStatusMessage(status), imageResult);
            }
            else
            {
                log.LogError($"The blob {blockBlob.Name} is missing its {ConfigSettings.JOBID_METADATA_NAME} metadata can't update the job");
            }
        }
    }
}
