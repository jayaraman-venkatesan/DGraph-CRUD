// See https://aka.ms/new-console-template for more information


using Dgraph;
using Dgraph.Api;
using Grpc.Net.Client;

namespace DgraphExample
{
    public class DgraphManager
    {
        private readonly IDgraphClient _dgraphClient;

        public DgraphManager(string address)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(address);
            _dgraphClient = DgraphClient.Create(channel);
        }

        public async Task<string> CheckDgraphVersion()
        {
            var version = await _dgraphClient.CheckVersion();
            return version.IsSuccess ? version.Value : "Unknown";
        }

        public async Task<bool> CreateSchema()
        {
            var operation = new Operation
            {
                Schema = "main_key: string @index(exact) .\n" +
                         "second_key: string @index(exact) .\n" +
                         "value_store: string @index(exact) .\n",
                RunInBackground = true
            };

            var response = await _dgraphClient.Alter(operation);
            return response.IsSuccess;
        }

        public async Task<bool> InsertData(string dataJson)
        {
            var transaction = _dgraphClient.NewTransaction();
            var mutation = new MutationBuilder().SetJson(dataJson).CommitNow();
            var response = await transaction.Mutate(mutation);
            return response.IsSuccess;
        }
        

        public async Task<bool> QueryData(string query)
        {
            var txn = _dgraphClient.NewReadOnlyTransaction();
            var response = await txn.Query(query);
            return response.IsSuccess;
        }

        public async Task<bool> UpdateData(string query, string mutationJson)
        {
            var mutation = new MutationBuilder().SetJson(mutationJson).CommitNow();
            var request = new RequestBuilder()
                .WithQuery(query)
                .WithMutations(mutation)
                .CommitNow();

            var txn = _dgraphClient.NewTransaction();
            var response = await txn.Do(request);
            return response.IsSuccess;
        }
        
        public async Task<bool> DeleteData(string query , string mutationJson)
        {
           
            var mutation = new MutationBuilder().DelNquads("uid(v) <main_key> * .").CommitNow();
            var request = new RequestBuilder()
                .WithQuery(query)
                .WithMutations(mutation)
                .CommitNow();

            var txn = _dgraphClient.NewTransaction();
            var response = await txn.Do(request);
            return response.IsSuccess;
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var dgraphManager = new DgraphManager("http://127.0.0.1:9080");

            var version = await dgraphManager.CheckDgraphVersion();
            Console.WriteLine("Dgraph Version: " + version);

            var schemaCreated = await dgraphManager.CreateSchema();
            Console.WriteLine("Schema Created: " + schemaCreated);

            var dataJson = "[{\"main_key\":\"1\",\"second_key\":\"1\",\"value_store\":\"2\"}," +
                           "{\"main_key\":\"2\",\"second_key\":\"2\",\"value_store\":\"3\"}," +
                           "{\"main_key\":\"3\",\"second_key\":\"3\",\"value_store\":\"5\"}," +
                           "{\"main_key\":\"4\",\"second_key\":\"4\",\"value_store\":\"9\"}]";
            var dataInserted = await dgraphManager.InsertData(dataJson);
            Console.WriteLine("Data Inserted: " + dataInserted);
            
     

            var query = @"
              query get_by_main_key {
                all(func: eq(main_key, 2)) {
                  main_key,
                  second_key,
                  value_store
                }
              }";
            
            var dataQueried = await dgraphManager.QueryData(query);
            Console.WriteLine("Data Queried: " + dataQueried);

            var updateQuery = @"
              query updateQuery{
                v as var(func: le(main_key, 5))
              }";
            var updateMutationJson = "{\"value_store\": 1069,\"uid\": \"uid(v)\"}";
            var dataUpdated = await dgraphManager.UpdateData(updateQuery, updateMutationJson);
            Console.WriteLine("Data Updated: " + dataUpdated);
            
            var deleteQuery = @"
              query deleteTest{
                v as var(func: eq(main_key, 1))
              }";
            var deleteDataJson = "{\"value_store\":.* \n \"uid\": \"uid(v)\"}";
            var dataDeleted = await dgraphManager.DeleteData(deleteQuery,deleteDataJson);
            Console.WriteLine("Data deleted: " +dataDeleted);
        }
    }
}