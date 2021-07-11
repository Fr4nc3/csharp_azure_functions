using AzureFunctions.Common;
using AzureFunctions.DTO;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureFunctions
{
    /// <summary>
    /// Serverless function to get a job entity 
    /// </summary>
    public static class ConversionJobStatusById
    {
 
        const string MyRoute = "v1/jobs/{id}";
        [FunctionName("ConversionJobStatusById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = MyRoute)] HttpRequest req, string id,
            ILogger log)
        {
            // check if id is defined and not null 
            ErrorResponse errorResponse = new ErrorResponse();
            if (id == null)
            {

                log.LogWarning(LoggingEvents.GetItemError, $"get JobEntity Id is null");
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterNoNull}");
                errorResponse.parameterName = "id";
                errorResponse.parameterValue = null;
                return new BadRequestObjectResult(errorResponse);

            }
            if (id == "")
            {
                log.LogWarning(LoggingEvents.GetItemError, $"get JobEntity Id is empty");
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterRequired}");
                errorResponse.parameterName = "id";
                errorResponse.parameterValue = id.ToString();
                return new BadRequestObjectResult(errorResponse);

            }
            Guid idGuid; // validade it is a guid value
            try
            {
                idGuid = new Guid(id);
            }
            catch
            {
                idGuid = Guid.Empty;
            }
            if (idGuid == Guid.Empty)
            {
                log.LogWarning(LoggingEvents.GetItemError, $"get JobEntity Id invalid");
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterRequired}");
                errorResponse.parameterName = "id";
                errorResponse.parameterValue = id;
                return new BadRequestObjectResult(errorResponse);
            }

            // get the table
            JobTable table = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            // get job dto
            var jobEntity = await table.RetrieveJobEntity(id);

            if (jobEntity == null)
            {
                log.LogWarning(LoggingEvents.GetItemNotFound, $"get JobEntity Id is {id} no found");
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                errorResponse.parameterName = "id";
                errorResponse.parameterValue = id;
                return new NotFoundObjectResult(errorResponse);
            }
            log.LogInformation(LoggingEvents.GetItem, $"get JobEntity exist {id}");
            var jobDto = new JobDto()
            {
                jobId = jobEntity.jobId,
                ImageConversionMode = jobEntity.imageConversionMode,
                Status = jobEntity.status,
                StatusDescription = jobEntity.statusDescription,
                ImageSource = jobEntity.imageSource,
                ImageResult = jobEntity.imageResult
            };
            return new OkObjectResult(jobDto);
        }
    }
}

