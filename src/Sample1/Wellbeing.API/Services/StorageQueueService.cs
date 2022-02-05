using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Wellbeing.API.Services;

public class StorageQueueService
{
    public static async Task QueueMessageAsync(IAsyncCollector<OutgoingMessage> msg, string email,
        string responseMessage)
    {
        var message = new OutgoingMessage
        {
            Target = "CorrespondenceService",
            Data = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["ResponseMessage"] = responseMessage
            }
        };

        await msg.AddAsync(message);
    }
}