using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

namespace AzureFunctions.Models
{
    /// <summary>
    /// Job Entity extend table Entity
    /// </summary>
    public class JobEntity : TableEntity
    {
        [StringLength(36)]
        public string jobId { get; set; }
        [StringLength(36)]
        public string blobName { get; set; }
        [StringLength(9)]
        public string imageConversionMode { get; set; }
        public int status { get; set; }

        [StringLength(512)]
        public string statusDescription { get; set; }
        [StringLength(512)]
        public string imageSource { get; set; }
        [StringLength(512)]
        public string imageResult { get; set; }
    }
}
