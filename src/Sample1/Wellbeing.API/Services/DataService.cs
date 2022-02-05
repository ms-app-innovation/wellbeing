using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Wellbeing.API.Services;

public class DataService
{
    public static async Task SaveStateAsync(
        CosmosClient cosmosClient,
        WellBeingStatus status)
    {
        var container = cosmosClient.GetDatabase("wellbeing-db").GetContainer("recommendation");
        await container.UpsertItemAsync(
            status, new PartitionKey(status.Email), new ItemRequestOptions()
            {
                IfMatchEtag = status.ETag
            });
    }

    public static async Task<WellBeingStatus> FetchAsync(CosmosClient cosmosClient, string id)
    {
        var container = cosmosClient.GetDatabase("wellbeing-db").GetContainer("recommendation");
        try
        {
            var item = await container.ReadItemAsync<WellBeingStatus>(
                id, 
                new PartitionKey(id));

            return item.Resource;
        }
        catch (CosmosException ce)
        {
            if (ce.StatusCode == HttpStatusCode.NotFound) return null;
            throw;
        }
    }
}