using AzureFunctions.Models;

namespace AzureFunctions.DTO
{
    public class ImageConvertionMode
    {

        /// <summary>
        /// Converts an imageMode enum inside an encoded imageMode 
        /// </summary>
        /// <param name="imageMode">The imageMode description</param>
        /// <returns>The decoded job status message and number</returns>
        public static string GetimageModeMessage(ImageMode imageMode)
        {

            int imageModeNumber = (int)imageMode;
            // list of job status imageMode
            switch (imageModeNumber)
            {
                case 1:
                    {
                        return "GrayScale";
                    }
                case 2:
                    {
                        return "Sepia";
                    }

                default:
                    {
                        return "Image Mode NO Implemented";
                    }

            }
        }
    }
}
