using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wellbeing.API.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;
        public EmailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string emailId, string responseMessage)
        {
            var emailObject = new { EmailAddress = emailId, EmailSubject = "Hi", EmailMessage = responseMessage };
            var content = new StringContent(JsonConvert.SerializeObject(emailObject), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GetEnvironmentVariable("EmailServiceUrl"), content);
            if (response.StatusCode != HttpStatusCode.Accepted)
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

