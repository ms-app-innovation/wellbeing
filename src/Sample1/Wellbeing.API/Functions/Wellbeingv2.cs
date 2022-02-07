using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Wellbeing.API.Domain;
using Wellbeing.API.Providers;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions;

public class Wellbeingv2
{
    private readonly CorrespondenceService _correspondenceService;
    private readonly ILogger<Wellbeingv2> _logger;


    public Wellbeingv2(ILogger<Wellbeingv2> log, CorrespondenceService correspondenceService)
    {
        _logger = log;
        _correspondenceService = correspondenceService;
    }


    [FunctionName("Wellbeing-v2")]
    [OpenApiOperation("Run", "name")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Parameters), Description = "Parameters", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")]
        CosmosClient cosmosClient,
        [Queue("wellbeing-email-q")] [StorageAccount("StorageConnectionString")]
        IAsyncCollector<OutgoingMessage> msg)
    {
        _logger.LogInformation("Wellbeing API has been called.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email)
            ? "Invalid Inputs."
            : new RecommendationProvider(name, score).Recommendation;

        await StorageQueueService.QueueMessageAsync(msg, email, responseMessage);

        await DataService.SaveStateAsync(cosmosClient,
            new WellBeingStatus
            {
                Name = name,
                Score = score,
                Email = email,
                Recommendation = responseMessage
            });


        _logger.LogInformation("Wellbeing API has been processed the recommendation.");

        return new JsonResult(responseMessage);
    }

    [FunctionName("WellbeingEmailer")]
    public async Task WellbeingEmailer(
        [QueueTrigger("wellbeing-email-q")] [StorageAccount("StorageConnectionString")]
        OutgoingMessage message,
        ILogger log)
    {
        if (message.Target == "CorrespondenceService")
            await _correspondenceService.SendEmailAsync(
                message.Data["Email"],
                "Task Queue",
                message.Data["ResponseMessage"]);
        log.LogInformation("C# queue emails sent");
    }
}