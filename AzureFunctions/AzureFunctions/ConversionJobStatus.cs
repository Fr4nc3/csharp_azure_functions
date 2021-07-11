using AzureFunctions.Models;
using AzureFunctions.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureFunctions
{
    /// <summary>
    /// serverless function to get all the jobs elements 
    /// </summary>
    public static class ConversionJobStatus
    {
        const string MyRoute = "v1/jobs";
        [FunctionName("ConversionJobStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = MyRoute)] HttpRequest req,
            ILogger log)
        {
            // get all the entities from the table
            var entities = await ImageConvertionService.GetJobEntities(log);

            return new OkObjectResult(entities);
        }
    }
}

