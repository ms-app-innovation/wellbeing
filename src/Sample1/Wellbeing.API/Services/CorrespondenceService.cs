using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class CorrespondenceService
{
    private readonly HttpClient _httpClient;

    public CorrespondenceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendEmailAsync(string emailId, string responseMessage)
    {
        var emailObject = new { EmailAddress = emailId, EmailSubject = "Hi", EmailMessage = responseMessage };
        var content = new StringContent(JsonConvert.SerializeObject(emailObject), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(GetEnvironmentVariable("EmailServiceUrl"), content);
        if (response.StatusCode != HttpStatusCode.Accepted) throw new Exception("Unable to send email");
    }

    private static string GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}