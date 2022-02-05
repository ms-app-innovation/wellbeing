using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Wellbeing.API.Services
{
    public class StorageQueueService
    {
        public async static Task QueueMessageAsync(IAsyncCollector<OutgoingMessage> msg, string email, string responseMessage)
        {
            //var message = JsonConvert.SerializeObject(new { email, responseMessage });

            var message = new OutgoingMessage()
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
}
