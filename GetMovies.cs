using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MongoExample
{
    public static class GetMovies
    {
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(() =>{
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URL"));
        });

        public static MongoClient client = lazyClient.Value;

        [FunctionName("GetMovies")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string limit = req.Query["limit"];

            IMongoCollection<Movie> moviesCollection = client
                .GetDatabase("sample_mflix")
                .GetCollection<Movie>("movies");

            BsonDocument filter = new BsonDocument {
                {
                    "year", new BsonDocument {
                        { "$gt", 2005 },
                        { "$lt", 2010 }
                    }
                }
            };

            var moviesToFind = moviesCollection.Find(filter);
            
            if (limit != null && Int32.Parse(limit) > 0)
            {
                moviesToFind.Limit(Int32.Parse(limit));
            }

            List<Movie> movies = moviesToFind.ToList();

            return new OkObjectResult(movies);
        }
    }
}

[BsonIgnoreExtraElements]
public class Movie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [BsonElement("plot")]
    [JsonPropertyName("plot")]
    public string Plot { get; set; } = null!;
}