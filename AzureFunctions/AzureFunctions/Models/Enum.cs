namespace AzureFunctions.Models
{
    /// <summary>
    // Error Code Enum
    /// </summary>
    public enum ErrorCode
    {
        EntityAlreadyExist = 1,
        ParameterRequired = 2,
        EntityNoFound = 3,
        ParameterNoNull = 4,
        ServerError = 5
    }
    /// <summary>
    ///  Job Status emi,
    /// </summary>
    public enum JobStatus
    {

        ImageObtained = 1,
        ImageBeingConverted = 2,
        ImageConvertedSuccess = 3,
        ImageConvertedFailed = 4,
        ImageSuccessDeteleSource = 5,
    }
    /// <summary>
    ///  ImageMode enum
    /// </summary>
    public enum ImageMode
    {
        GrayScale = 1,
        Sepia = 2
    }

}
