using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDBSQLApi
{
    class Program
    {
        private const string _endpointUrl = "https://ghfdfsg.documents.azure.com:443/";
        private const string _primaryKey = "HNUEovKYJ1gM92aHxDW1yUU98Sbf9ZSyMJN97zmSHgxV9rOJpKVd34VYsjdXGdiubDG6r50DgrbbZwjUviMx8Q==";
        private DocumentClient _client;
        private string _databaseName = "FamilyDB";
        private string _collectionName = "FamilyCollection";

        static void Main(string[] args)
        {
            Program p = new Program();
            p.GetStartedDemo().Wait();
        }
        private async Task GetStartedDemo()
        {
            _client = new DocumentClient(new Uri(_endpointUrl), _primaryKey);

            //when you insert this family the trigger gets executed
            Family condrea = new Family() { Id = "aaaaab", Name = "wcaldsauzitk" };
            await CreateFamilyDocumentIfNotExists(_databaseName, _collectionName, condrea);
            //await CreateTrigger();
        }

        private async Task CreateTrigger()
        {
            Trigger triger = new Trigger();
            triger.Id = "trigerId";
            triger.Body = @"function testTrigger() {
            var context = getContext();
            var request = context.getRequest();
            var documentToCreate = request.getBody();
            documentToCreate['test'] = 'Added by trigger';
            request.setBody(documentToCreate);
            throw new Error('da meregedf');
            }";
            triger.TriggerOperation = TriggerOperation.Create;
            triger.TriggerType = TriggerType.Pre;

            await _client.CreateTriggerAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName), triger, null);
    }


        // ADD THIS PART TO YOUR CODE
        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, Family family)
        {
            try
            {
                await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, family.Id), new RequestOptions { PartitionKey = new PartitionKey("/p")});
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    //MUST assign to every document if you whant it to run
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), family,
                        new RequestOptions
                        {
                            PreTriggerInclude = new List<string> { "trigerId" }
                        });
                }
                else
                {
                    throw;
                }
            }
        }
    }

    // ADD THIS PART TO YOUR CODE
    public class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
