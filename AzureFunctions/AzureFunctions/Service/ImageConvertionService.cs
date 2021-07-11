using AzureFunctions.Common;
using AzureFunctions.DTO;
using AzureFunctions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctions.Service
{
    public class ImageConvertionService
    {
        /// <summary>
        /// Converts the and store image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImagesContainer">The uploaded images container.</param>
        /// <param name="convertedImagesContainer">The converted images container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        public static async Task ConvertAndStoreImage(ILogger log,
                                                 Stream uploadedImage,
                                                 CloudBlobContainer convertedImagesContainer,
                                                 string blobName,
                                                 CloudBlobContainer failedImagesContainer,
                                                 string jobId, ImageMode imageMode)
        {
            string convertedBlobName = $"{Guid.NewGuid()}-{blobName}";

            JobStatus status = JobStatus.ImageBeingConverted;
            await UpdateJobTableWithStatus(log, jobId, status: status, message: JobStatusDescription.GetjobStatusMessage(status));
            try
            {
                uploadedImage.Seek(0, SeekOrigin.Begin);

                using (MemoryStream convertedMemoryStream = new MemoryStream())
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(uploadedImage))
                {
                    log.LogInformation($"[+] Starting conversion of image {blobName}");
                    // set the image mode grayscale or sepia 
                    if (imageMode == ImageMode.GrayScale)
                    {
                        image.Mutate(x => x.Grayscale());
                    }
                    else
                    {
                        image.Mutate(x => x.Sepia());
                    }

                    image.SaveAsJpeg(convertedMemoryStream);

                    convertedMemoryStream.Seek(0, SeekOrigin.Begin);
                    log.LogInformation($"[-] Completed conversion of image {blobName}");

                    log.LogInformation($"[+] Storing converted image {blobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                    CloudBlockBlob convertedBlockBlob = convertedImagesContainer.GetBlockBlobReference(convertedBlobName);

                    convertedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, jobId);

                    convertedBlockBlob.Properties.ContentType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    await convertedBlockBlob.UploadFromStreamAsync(convertedMemoryStream);
                    log.LogInformation($"[-] Stored converted image {convertedBlobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to convert blob {blobName} Exception ex {ex.Message}");
                await StoreFailedImage(log, uploadedImage, blobName, failedImagesContainer, convertedBlobName: convertedBlobName, jobId: jobId);
            }
        }
        /// <summary>
        /// Updates/creates the job table with status.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        /// <param name="url">The url.</param>
        /// <param name=" imageMode">The  imageMode.</param>
        public static async Task UpdateJobTableWithStatus(ILogger log, string jobId, JobStatus status, string message, string url = "", string blobName = "", ImageMode imageMode = ImageMode.GrayScale)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            switch (status)
            {
                case JobStatus.ImageObtained:
                    log.LogInformation($"Create {jobId} {(int)status} {message} {blobName} {imageMode}");
                    await jobTable.InsertOrReplaceJobEntity(jobId, status: status, message: message, url, blobName, imageMode);
                    break;
                case JobStatus.ImageBeingConverted:
                case JobStatus.ImageConvertedSuccess:
                case JobStatus.ImageConvertedFailed:
                    log.LogInformation($"Update {jobId} {(int)status} {message}");
                    await jobTable.UpdateJobEntityStatus(jobId, status: status, message: message, url);
                    break;
            }


        }

        public static async Task<JobDto[]> GetJobEntities(ILogger log)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            log.LogInformation("Get All Job Entities from", ConfigSettings.JOBS_TABLENAME);

            // the table
            JobTable table = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            // get all the jobs entities
            var entities = await table.RetrieveJobs();

            // convert JobEntities to JobDto to show on the solution
            return entities.Select(x => new JobDto
            {
                jobId = x.jobId,
                ImageConversionMode = x.imageConversionMode,
                Status = x.status,
                StatusDescription = x.statusDescription,
                ImageSource = x.imageSource,
                ImageResult = x.imageResult
            }).ToArray();


        }

        /// <summary>
        /// Stores the failed image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImage">The uploaded image.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="failedImagesContainer">The failed images container.</param>
        /// <param name="convertedBlobName">Name of the converted BLOB.</param>
        /// <param name="jobId">The job identifier.</param>
        public static async Task StoreFailedImage(ILogger log, Stream uploadedImage, string blobName, CloudBlobContainer failedImagesContainer, string convertedBlobName, string jobId)
        {
            try
            {
                log.LogInformation($"[+] Storing failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");

                CloudBlockBlob failedBlockBlob = failedImagesContainer.GetBlockBlobReference(convertedBlobName);
                failedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, jobId);

                uploadedImage.Seek(0, SeekOrigin.Begin);
                await failedBlockBlob.UploadFromStreamAsync(uploadedImage);

                log.LogInformation($"[+] Stored failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to store a blob called {blobName} that failed conversion into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME}. Exception ex {ex.Message}");
            }
        }

        public static async Task DeleteUploadedImagesByStatus(ILogger log, JobStatus status)
        {
            // we don't touch images/file for the point 10 HW 4
            var excludeImages = new List<string>() { "Dogs.JPG", "seagul.jpg", "turkey.jpg", "usconstitution.pdf" };
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            log.LogInformation($"Get All Job Entities from {ConfigSettings.JOBS_TABLENAME} we will delete");

            // the table
            JobTable table = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            // get all the jobs entities
            var entities = await table.RetrieveJobs();

           // we don't touch the one loaded from the howework point 10
           var filteredJobs = entities.FindAll(x => !excludeImages.Contains(x.blobName) && x.status == (int)status);
           
            log.LogInformation($"Delete Jobs from {ConfigSettings.JOBS_TABLENAME} ");
            filteredJobs.ForEach(async x =>
            {
                log.LogInformation($"Delete Jobs from {x.blobName} {x.statusDescription} ");
                string containername = x.imageConversionMode == ImageConvertionMode.GetimageModeMessage(ImageMode.GrayScale) ? ConfigSettings.GRAYSCALEIMAGES_CONTAINERNAME : ConfigSettings.SEPIAIMAGES_CONTAINERNAME;
                try
                {
                    log.LogInformation($"Try delete Jobs from {containername} {x.imageConversionMode} ");
                    // Retrieve a reference to a container. 
                    CloudBlobContainer container = GetContainer(containername);

                    bool containerExist = await ContainerExist(container);

                    if (containerExist) // if container exist
                    {
                        var statusNew = JobStatus.ImageSuccessDeteleSource;
                        await UpdateJobTableWithStatus(log, x.jobId, status: statusNew, message: JobStatusDescription.GetjobStatusMessage(statusNew)); // update job status  
                        log.LogInformation($"Container Exist {containername} {x.imageConversionMode} {x.blobName} ");
                        // Retrieve reference to a blob named the blob specified by the caller
                        CloudBlockBlob blockBlob = GetCloudBlockBlob(container, x.blobName);

                        log.LogInformation($"File Exist {containername} {x.imageConversionMode} {x.blobName} ");
                        await blockBlob.DeleteIfExistsAsync(); // delete file from container
                    }
                }
                catch (StorageException se) // storage exception
                {
                    log.LogError(LoggingEvents.UpdateItemError, $"Error Delete fileName = {x.blobName}, containername = {containername}", se.Message);
                }
                catch (Exception ex) // server error
                {
                    log.LogError(LoggingEvents.UpdateItemError, $"Error Delete fileName = {x.blobName}, containername = {containername}", ex.Message);
                }

            });

        }
        public static CloudBlobContainer GetContainer(string containername)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(containername);

            return container;

        }
        public static CloudBlockBlob GetCloudBlockBlob(CloudBlobContainer container, string fileName)
        {
            // Retrieve reference to a blob named the blob specified by the caller
            return container.GetBlockBlobReference(fileName);

        }
        public static async Task<bool> BlockBlobExist(CloudBlockBlob blockBlob)
        {
            // if blockblob is null
            if (blockBlob == null)
            {
                return false;
            }
            // file exist or not
            bool fileExist = await blockBlob.ExistsAsync();

            return fileExist;

        }

        public static async Task<bool> ContainerExist(CloudBlobContainer container)
        {
            // if container is null
            if (container == null)
            {
                return false;
            }
            // container exist or not
            bool containerExist = await container.ExistsAsync();

            return containerExist;

        }
    }
}
