using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Wellbeing.API.Functions.EventSourcingReadModelPopulators;

public static class CosmosEx
{
    public static async Task<T> NoThrow<T>(this Task<ItemResponse<T>> task)
    {
        try
        {
            var response = await task;
            return response.Resource;
        }
        catch (CosmosException ce)
        {
            if (ce.StatusCode == HttpStatusCode.NotFound) return default;
            throw;
        }
    }
}