

using Dgraph;
using Dgraph.Api;
using Grpc.Net.Client;



AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:9080");
using var dgraphClient = DgraphClient.Create(channel);


var version = await dgraphClient.CheckVersion();

if (version.IsSuccess)
{
    Console.WriteLine("Version: " + version.Value);
}


// Create table

var operation = new Operation {
    Schema = "main_key: string @index(exact) .\n" +
             "second_key: string @index(exact) .\n" +
             "value_store: string @index(exact) .\n",
    RunInBackground = true
};

var response = await dgraphClient.Alter(operation);
if (response.IsFailed) {
    // Handle errors
}

// insert table

String query =
    "[{\"main_key\":\"1\",\"second_key\":\"1\",\"value_store\":\"2\"},{\"main_key\":\"2\",\"second_key\":\"2\",\"value_store\":\"3\"},{\"main_key\":\"3\",\"second_key\":\"3\",\"value_store\":\"5\"},{\"main_key\":\"4\",\"second_key\":\"4\",\"value_store\":\"9\"}]";


var transaction = dgraphClient.NewTransaction();

var mutation1 = new MutationBuilder().SetJson(query).CommitNow();

var response2 = await transaction.Mutate(mutation1);

if (response2.IsFailed) {
    // Handle errors
}

// Read (query)
var query3 = @"
  query get_by_main_key {
    all(func: eq(main_key, 2)) {
      main_key,
      second_key,
      value_store
    }
  }";

var txn3 = dgraphClient.NewReadOnlyTransaction();

var response3 = await txn3.Query(query3);
if (response3.IsSuccess)
{
    Console.WriteLine("SUCCESS\n");
}

// Read Range
var query4 = @"
  query readRange {
    all(func: le(main_key, 5)) {
      main_key,
      second_key,
      value_store
    }
  }";



var response4 = await txn3.Query(query3);
if (response4.IsSuccess)
{
    Console.WriteLine("SUCCESS - Response 4\n");
}




// update
var query6 = @"
  query {
    v as var(func: le(main_key, 5))
  }";
var mutation3 = new MutationBuilder().SetJson("{\"value_store\": 1069,\"uid\": \"uid(v)\"}").CommitNow();

var request = new RequestBuilder()
    .WithQuery(query6)
    .WithMutations(mutation3)
    .CommitNow();

// Update email only if exactly one matching uid found.
var txnNew = dgraphClient.NewTransaction();
var response123 = await txnNew.Do(request);
if (response123.IsFailed) {
    // Handle errors
}



Console.WriteLine("hello");