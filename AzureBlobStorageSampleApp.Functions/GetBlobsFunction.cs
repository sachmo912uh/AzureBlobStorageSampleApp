using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;

using AzureBlobStorageSampleApp.Backend.Common;

namespace AzureBlobStorageSampleApp.Functions
{
    public static class GetBlobsFunction
    {
        [FunctionName("GetBlobFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                var photoList = await PhotoDatabaseService.GetAllPhotos();
            }
        }
    }
}
