using AzureFunctions.Common;
using AzureFunctions.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctions.Models
{
    /// <summary>
    /// Job Table model and methods
    /// </summary>
    public class JobTable
    {
        private CloudTableClient _tableClient;
        private CloudTable _table;
        private string _partitionKey;
        private ILogger _log;

        public JobTable(ILogger log, string partitionKey)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the table client.
            _tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "jobentity" table.
            _table = _tableClient.GetTableReference(ConfigSettings.JOBS_TABLENAME);

            _table.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            _partitionKey = partitionKey;

            _log = log;
        }

        /// <summary>
        /// Retrieves the job entity.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>JobEntity.</returns>
        public async Task<JobEntity> RetrieveJobEntity(string jobId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<JobEntity>(_partitionKey, jobId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            return retrievedResult.Result as JobEntity;
        }

        /// <summary>
        /// Retrieves the job entitentities .
        /// </summary>
        /// <returns>JobEntities .</returns>
        public async Task<List<JobEntity>> RetrieveJobs()
        {
            List<JobEntity> entities = new List<JobEntity>();

            // Construct the query operation for all internal log entities 
            TableQuery<JobEntity> query = new TableQuery<JobEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey));

            // Get all of the job entities
            TableContinuationToken token = new TableContinuationToken();

            foreach (JobEntity entity in await _table.ExecuteQuerySegmentedAsync(query, null))
            {
                _log.LogInformation(LoggingEvents.GetItemList, $"Get Item fileName = {entity.jobId}, statusDesc = {entity.statusDescription}");
                entities.Add(entity);
            }

            return (List<JobEntity>)entities.AsEnumerable();
        }

        /// <summary>
        /// Retrieves the job entitentities .
        /// </summary>
        /// <returns>JobEntities .</returns>
        public async Task<List<JobEntity>> RetrieveJobsByStatus(JobStatus status)
        {
            List<JobEntity> entities = new List<JobEntity>();
            // filter one partionkey
            var filter1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey);
            // filter 2 success 
            var filter2 = TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, $"{(int)status}");
            var combineFilter = TableQuery.CombineFilters( filter1,TableOperators.And, filter2);
            // Construct the query operation for all internal log entities 
            TableQuery<JobEntity> query = new TableQuery<JobEntity>().Where(combineFilter);

            // Get all of the job entities
            TableContinuationToken token = new TableContinuationToken();

            foreach (JobEntity entity in await _table.ExecuteQuerySegmentedAsync(query, null))
            {
                _log.LogInformation(LoggingEvents.GetItemList, $"GetItem to Delete Item fileName = {entity.jobId}, statusDesc = {entity.statusDescription}");
                entities.Add(entity);
            }

            return (List<JobEntity>)entities.AsEnumerable();
        }

        /// <summary>
        /// Updates the job entity.
        /// </summary>
        /// <param name="jobEntity">The job entity.</param>
        public async Task<bool> UpdateJobEntity(JobEntity jobEntity)
        {
            TableOperation replaceOperation = TableOperation.Replace(jobEntity);
            TableResult result = await _table.ExecuteAsync(replaceOperation);

            if (result.HttpStatusCode > 199 && result.HttpStatusCode < 300)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the job entity status.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        /// <param name="imageSource">The imageResult.</param>
        public async Task UpdateJobEntityStatus(string jobId, JobStatus status, string message, string imageResult)
        {
            JobEntity jobEntityToReplace = await RetrieveJobEntity(jobId);
            if (jobEntityToReplace != null)
            {
                jobEntityToReplace.status = (int)status;
                jobEntityToReplace.statusDescription = message;
                if (!String.IsNullOrEmpty(imageResult))
                {
                    jobEntityToReplace.imageResult = imageResult;
                }

                await UpdateJobEntity(jobEntityToReplace);
            }
        }

        /// <summary>
        /// Inserts the or replace job entity.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        /// <param name="imageSource">The imageSource.</param>
        /// <param name="imageMode">The imageMode.</param>
        public async Task InsertOrReplaceJobEntity(string jobId, JobStatus status, string message, string imageSource, string blobName, ImageMode imageMode)
        {

            JobEntity jobEntityToInsertOrReplace = new JobEntity();
            jobEntityToInsertOrReplace.RowKey = jobId;
            jobEntityToInsertOrReplace.PartitionKey = _partitionKey;
            jobEntityToInsertOrReplace.status = (int)status;
            jobEntityToInsertOrReplace.statusDescription = message;
            jobEntityToInsertOrReplace.jobId = jobId;
            if (!String.IsNullOrEmpty(imageSource))
            {
                jobEntityToInsertOrReplace.imageSource = imageSource;
            }
            if (!String.IsNullOrEmpty(blobName))
            {
                jobEntityToInsertOrReplace.blobName = blobName;
            }

            jobEntityToInsertOrReplace.imageConversionMode = ImageConvertionMode.GetimageModeMessage(imageMode);

            TableOperation insertReplaceOperation = TableOperation.InsertOrReplace(jobEntityToInsertOrReplace);
            TableResult result = await _table.ExecuteAsync(insertReplaceOperation);

        }

    }
}
