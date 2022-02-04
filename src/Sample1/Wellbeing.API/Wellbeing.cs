using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Wellbeing.API
{
    public class Wellbeing
    {
        private readonly ILogger<Wellbeing> _logger;
        private readonly HttpClient _httpClient;


        public Wellbeing(ILogger<Wellbeing> log, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = log;
        }

        [FunctionName("Wellbeing")]
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

            await SendEmailAsync(email, responseMessage);

            await SaveStateAsync(cosmosClient, name, score, email, responseMessage);


            _logger.LogInformation("Wellbeing API has been processed the recommendation.");

            return new JsonResult(responseMessage);
        }

        private static async Task SaveStateAsync(DocumentClient cosmosClient, string name, int score, string email, string responseMessage)
        {
            var doc = new { id = email, Name = name, Email = email, Score = score, Recommendation = responseMessage };
            await cosmosClient.UpsertDocumentAsync("dbs/wellbeing-db/colls/recommendation/", doc);
        }

        private async Task SendEmailAsync(string emailId, string responseMessage)
        {
            var emailObject = new { EmailAddress = emailId, EmailSubject = "Hi", EmailMessage = responseMessage };
            var content = new StringContent(JsonConvert.SerializeObject(emailObject), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GetEnvironmentVariable("EmailServiceUrl"), content);
            if(response.StatusCode != HttpStatusCode.Accepted)
            { 
                throw new Exception("Unable to send email");
            }
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}

