using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Wellbeing.API.Providers;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions
{
    public class Wellbeingv2
    {
        private readonly ILogger<Wellbeingv2> _logger;
        private readonly EmailService CorrespondenceService;


        public Wellbeingv2(ILogger<Wellbeingv2> log, HttpClient httpClient)
        {
            _logger = log;
            CorrespondenceService = new EmailService(httpClient);
        }


        [FunctionName("Wellbeing-v2")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Parameters), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "wellbeing-db", collectionName: "wellbeing", ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient cosmosClient)
        {
            _logger.LogInformation("Wellbeing API has been called.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data["name"];
            int score = Convert.ToInt32(data["score"]);
            string email = data["email"];

            string responseMessage = string.IsNullOrEmpty(email)
                ? "Invalid Inputs."
                : new RecommendationProvider(name, score).Recommendation;

            await CorrespondenceService.SendEmailAsync(email, responseMessage);

            await DataService.SaveStateAsync(cosmosClient, name, score, email, responseMessage);


            _logger.LogInformation("Wellbeing API has been processed the recommendation.");

            return new JsonResult(responseMessage);
        }           
    }
}

