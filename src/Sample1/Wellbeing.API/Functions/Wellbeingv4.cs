using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
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

/// <summary>
///     This version uses an Outbox pattern, and a dispatcher, to handle the call to the Correspondence Service
/// </summary>
public class Wellbeingv4
{
    private readonly CorrespondenceService _correspondenceService;
    private readonly ILogger<Wellbeingv4> _logger;

    public Wellbeingv4(ILogger<Wellbeingv4> log, CorrespondenceService correspondenceService)
    {
        _logger = log;
        _correspondenceService = correspondenceService;
    }


    [FunctionName("Wellbeing-v4")]
    [OpenApiOperation("Run", "name")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Parameters), Description = "Parameters", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")]
        CosmosClient cosmosClient)
    {
        _logger.LogInformation("Wellbeing API has been called");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email)
            ? "Invalid Inputs."
            : new RecommendationProvider(name, score).Recommendation;

        var existing = await DataService.FetchAsync(cosmosClient, email);
        existing ??= new WellBeingStatus
        {
            Email = email,
            Name = name,
            Score = score
        };
        existing.RecordNewWellbeingStatus(responseMessage, score, new OutgoingMessage()
        {
            Target = "Orchestrator",
            Id = Guid.NewGuid(),
            Data = new Dictionary<string, string>
            {
                ["Id"] = email,
                ["OrchestratorName"] = "Orchestrator",
                ["ResponseMessage"] = responseMessage
            }
        });

        await DataService.SaveStateAsync(cosmosClient, existing);

        _logger.LogInformation("Wellbeing API has been processed the recommendation using the Outbox pattern");

        return new JsonResult(responseMessage);
    }

    [FunctionName("Orchestrator")]
    public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var message = context.GetInput<OutgoingMessage>();

        var parallelTasks = new List<Task>();        

        parallelTasks.Add(context.CallActivityWithRetryAsync(
            "SendMessageActivity",
            new RetryOptions(TimeSpan.FromSeconds(1), 10),
            message));

        parallelTasks.Add(context.CallActivityWithRetryAsync(
            "BroadcastSentimentActivity",
            new RetryOptions(TimeSpan.FromSeconds(1), 10),
            message));

        await Task.WhenAll(parallelTasks);
    }

    [FunctionName("SendMessageActivity")]
    public async Task SendMessageActivity(
        [ActivityTrigger] IDurableActivityContext context)
    {
        var message = context.GetInput<OutgoingMessage>();        
        await _correspondenceService.SendEmailAsync(message.Data["Id"], message.Data["ResponseMessage"], "Durable Function Send Message Activity");
    }

    [FunctionName("BroadcastSentimentActivity")]
    public async Task BroadcastSentimentActivity(
    [ActivityTrigger] IDurableActivityContext context,
    [EventGrid(TopicEndpointUri = "EventGridTopicUri", TopicKeySetting = "EventGridTopicKey")] IAsyncCollector<EventGridEvent> outputEvents)
    {
        var message = context.GetInput<OutgoingMessage>();        
        await outputEvents.AddAsync(new EventGridEvent("subject", "eventType", "dataVersion", message));
    }
}