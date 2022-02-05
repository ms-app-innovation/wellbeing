using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Wellbeing.API.Providers;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions;

/// <summary>
///     This version uses an Outbox pattern, and a dispatcher, to handle the call to the Correspondence Service
/// </summary>
public class Wellbeingv4
{
    private readonly ILogger<Wellbeingv4> _logger;
    private readonly CorrespondenceService _correspondenceService;

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")] CosmosClient cosmosClient)
    {
        _logger.LogInformation("Wellbeing API has been called");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email) ? "Invalid Inputs." : new RecommendationProvider(name, score).Recommendation;

        var existing = await DataService.FetchAsync(cosmosClient, email);
        existing ??= new WellBeingStatus()
        {
            Email = email,
            Name = name,
            Score = score
        };
        existing.RecordNewWellbeingStatus(responseMessage, score);

        _logger.LogInformation("Wellbeing API has been processed the recommendation using the Outbox pattern");

        return new JsonResult(responseMessage);
    }

    [FunctionName("Orchestrator")]
    public async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        // retrieves the organization name from the Orchestrator_HttpStart function
        var message = context.GetInput<OutgoingMessage>();

        await Dispatch(_correspondenceService, message);
        return context.InstanceId;
    }

    private Task Dispatch(CorrespondenceService correspondenceService, OutgoingMessage message)
    {
        if (message.Target == "CorrespondenceService")
        {
            return correspondenceService.SendEmailAsync(
                message.Data["Email"],
                message.Data["ResponseMessage"]);
        }
        return Task.CompletedTask;
    }
}