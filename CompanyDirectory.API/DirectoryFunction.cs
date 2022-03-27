using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;

namespace CompanyDirectory.API
{
    public static class DirectoryFunction
    {
        [FunctionName("GetPersonnel")]
        public static async Task<IActionResult> GetPersonnel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = System.Environment.GetEnvironmentVariable("MongoConnectionString", EnvironmentVariableTarget.Process);

            var client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("directory");
            IMongoCollection<Personnel> itemsCollection = db.GetCollection<Personnel>("personnel");

            List<Personnel> items = await itemsCollection.Find(item => true).ToListAsync();

            return new OkObjectResult(items);
        }

        [FunctionName("LoadDemoPersonnel")]
        public static async Task<IActionResult> LoadDemoPersonnel(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
          ILogger log)
        {
            string connectionString = System.Environment.GetEnvironmentVariable("MongoConnectionString", EnvironmentVariableTarget.Process);

            var client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("directory");
            IMongoCollection<Personnel> itemsCollection = db.GetCollection<Personnel>("personnel");

            itemsCollection.InsertMany(new List<Personnel>
            {
                new Personnel
                {
                    FirstName= "Lorth",
                    LastName= "Needa",
                    Title= "Captain",
                    Image = "/images/needa.jpg",
                    Active= true,
                    Id =  5,
                    DetailsUrl= "http://www.starwars.com/databank/captain-needa"
                },
                  new Personnel
                  {
                    FirstName =  "Darth",
                    LastName =  "Vader",
                    Title =  "Sith Lord",
                    Image =  "/images/vader.jpg",
                    Active =  true,
                    Id =  2,
                    DetailsUrl =  "http://www.starwars.com/databank/darth-vader"
                  },
                   new Personnel
                  {
                    FirstName =  "Kendal",
                    LastName =  "Ozzel",
                    Title =  "Admiral",
                    Image =  "/images/ozzel.jpg",
                    Active =  true,
                    Id =  3,
                    DetailsUrl =  "http://www.starwars.com/databank/admiral-ozzel"
                  },
                    new Personnel
                  {
                    FirstName =  "Firmus",
                    LastName =  "Piett",
                    Title =  "Captain",
                    Image =  "/images/piett.jpg",
                    Active =  true,
                    Id =  4,
                    DetailsUrl =  "http://www.starwars.com/databank/admiral-piett"
                  },
                     new Personnel
                  {
                    FirstName =  "Wilhuff",
                    LastName =  "Tarkin",
                    Title =  "Grand Moff",
                    Image =  "/images/tarkin.jpg",
                    Active =  false,
                    Id =  1,
                    DetailsUrl =  "https://www.starwars.com/databank/grand-moff-tarkin"
                  }
            });


            return new OkResult();
        }
    }

    public class Personnel
    {
        public int Id { get; set; }

        public string Image { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Title { get; set; }

        public bool Active { get; set; }

        public string DetailsUrl { get; set; }
    }
}
