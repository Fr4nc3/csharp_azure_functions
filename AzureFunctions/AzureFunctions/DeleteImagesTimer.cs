using AzureFunctions.Models;
using AzureFunctions.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace AzureFunctions
{
    public static class DeleteImagesTimer
    {
        const string cron = "* */2 * * * *"; // every two minute
       // const string cron = "* */5 * * * *"; // every 5 minutes for test porpuse

        /// <summary>
        /// trigger time that run every two minutes
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName("DeleteImagesTimer")]
        public static async void Run([TimerTrigger(cron)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now} Delete source images from success");
            await ImageConvertionService.DeleteUploadedImagesByStatus(log, JobStatus.ImageConvertedSuccess);
        }
    }
}
