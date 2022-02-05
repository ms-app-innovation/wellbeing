using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;

namespace Wellbeing.API.Services;

public class DataService
{
    public static async Task SaveStateAsync(
        CosmosClient cosmosClient,
        WellBeingStatus status)
    {
        var container = cosmosClient.GetDatabase("wellbeing-db").GetContainer("recommendation");
        await container.UpsertItemAsync(status);
    }
}