# csharp_azure_functions

Azure Functions using C# Core .Net 3.1

Solution name AzureFunctions
open project and build no error or warning

## AzureDevops

- Create a Blob Container
- A File Service
- A Queue Service
- A Table Service

## Azure List functions

- ConvertionJobStatus
- ConvertionJobStatusById
- DeleteImagesTimer
- ImageConsumerGreyScale
- ImageConsumerSepia
- ImagesStatusUpdaterFailed
- ImagesStatusUpdaterSuccess

## Files

- Common

  - ConfigSettings
  - LoggingEvents

- DTO

  - ErrorResponse
  - ImageCOnvertionMode
  - JobDto
  - obStatusDescription

- Models
  - Enum
  - JObEntity
    JobTable
  - Services
  - ImageConvertionService

## local settings

```JSON
{
"IsEncrypted": false,
"Values": {
"AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=demo-fr2021;AccountKey=gy7bS44aTcPQgL0HmEwo1AMBJSi/0xHPZX0DsBUQHiTNMMeAYyn65UgXGPiL8O5usnv4LnOK8aKAVwFmbfzW+Q==;BlobEndpoint=https://demo-fr2021.blob.core.windows.net/;TableEndpoint=https://demo-fr2021.table.core.windows.net/;QueueEndpoint=https://demo-fr2021.queue.core.windows.net/;FileEndpoint=https://demo-fr2021.file.core.windows.net/",
"FUNCTIONS_WORKER_RUNTIME": "dotnet"
}
}
```

## delete source

delete images from the container converttogreyscale and converttosepia, except
time trigger functions DeleteImagesTimer

## References

https://docs.microsoft.com/en-us/azure/azure-functions/functions-best-practices
