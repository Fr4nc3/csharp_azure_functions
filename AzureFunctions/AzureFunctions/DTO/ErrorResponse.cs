using System.ComponentModel.DataAnnotations;

namespace AzureFunctions.DTO
{
    /// <summary>
    /// Error response class 
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// error number from defined list
        /// </summary>
        public int errorNumber { get; set; }
        /// <summary>
        /// parameterName which created the error
        /// </summary>
        [StringLength(1024)]
        public string parameterName { get; set; }
        /// <summary>
        /// the value of the parameter that failed 
        /// </summary>
        [StringLength(2048)]
        public string parameterValue { get; set; }
        /// <summary>
        /// errr description
        /// </summary>
        [StringLength(1024)]
        public string errorDescription { get; set; }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {

            int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);
            // list of valid errors
            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists", errorNumber);
                    }
                case 2:
                    {
                        return ("The parameter is required.", errorNumber);
                    }
                case 3:
                    {
                        return ("The entity could not be found", errorNumber);
                    }
                case 4:
                    {
                        return ("The parameter cannot be null", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }

            }
        }

    }
}
