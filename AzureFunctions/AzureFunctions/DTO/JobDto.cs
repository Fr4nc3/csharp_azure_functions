using System.ComponentModel.DataAnnotations;

namespace AzureFunctions.DTO
{
    /// <summary>
    /// Job Dto simple JobEntity version
    /// </summary>
    public class JobDto
    {
        [StringLength(36)]
        public string jobId { get; set; }
        [StringLength(9)]
        public string ImageConversionMode { get; set; }
        public int Status { get; set; }

        [StringLength(512)]
        public string StatusDescription { get; set; }
        [StringLength(512)]
        public string ImageSource { get; set; }
        [StringLength(512)]
        public string ImageResult { get; set; }
    }
}
