﻿using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace Wellbeing.API.Services;

public class DataService
{
    public static async Task SaveStateAsync(DocumentClient cosmosClient, string name, int score, string email,
        string responseMessage)
    {
        var doc = new { id = email, Name = name, Email = email, Score = score, Recommendation = responseMessage };
        await cosmosClient.UpsertDocumentAsync("dbs/wellbeing-db/colls/recommendation/", doc);
    }

    public static async Task SaveStateAsyncWithOutbox(
        DocumentClient cosmosClient, 
        string name, 
        int score, 
        string email,
        string responseMessage)
    {
        var doc = new
        {
            id = email, 
            Name = name, 
            Email = email, 
            Score = score, 
            Recommendation = responseMessage,
            Outbox = new []
            {
                new
                {
                    Target = "CorrespondenceService",
                    Email = email,
                    ResponseMessage = responseMessage
                }
            }
        };
        
        await cosmosClient.UpsertDocumentAsync("dbs/wellbeing-db/colls/recommendation/", doc);
    }
}
