using AzureFunctions.Common;
using AzureFunctions.DTO;
using AzureFunctions.Models;
using AzureFunctions.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public static class ImageConsumerGreyScale
    {
        const string ImagesToConvertRoute = ConfigSettings.GRAYSCALEIMAGES_CONTAINERNAME + "/{name}";

        /// <summary>
        /// Converts images uploaded into the imagestoconverttograyscale into gray scale format
        /// If success the convertedimages container contains the result image
        /// If fail the failedimages container contains the original image uploaded into the
        /// imagestoconverttograyscale continer
        /// 
        /// An initial job record is added to the jobs table indicating the status of the job
        /// </summary>
        /// <param name="blobStream">The BLOB stream.</param>
        /// <param name="name">The name.</param>
        /// <param name="log">The log.</param>

        [FunctionName("ImageConsumerGreyScale")]
        public static async Task Run([BlobTrigger(ImagesToConvertRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob cloudBlockBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n ContentType: {cloudBlockBlob.Properties.ContentType} Bytes");
            string jobId = Guid.NewGuid().ToString();

            using (Stream blobStream = await cloudBlockBlob.OpenReadAsync())
            {
                JobStatus status = JobStatus.ImageObtained;
                string imageSource = cloudBlockBlob.Uri.AbsoluteUri;
                ImageMode imageMode = ImageMode.GrayScale;
                await ImageConvertionService.UpdateJobTableWithStatus(log, jobId, status: status, message: JobStatusDescription.GetjobStatusMessage(status), imageSource, name, imageMode);
                // Get the storage account
                string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                // Create a blob client so blobs can be retrieved and created
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create or retrieve a reference to the converted images container
                CloudBlobContainer convertedImagesContainer = blobClient.GetContainerReference(ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME);
                bool created = await convertedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                CloudBlobContainer failedImagesContainer = blobClient.GetContainerReference(ConfigSettings.FAILED_IMAGES_CONTAINERNAME);
                created = await failedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigSettings.FAILED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                await ImageConvertionService.ConvertAndStoreImage(log, blobStream, convertedImagesContainer, name, failedImagesContainer, jobId, imageMode);
            }
        }

    }
}

