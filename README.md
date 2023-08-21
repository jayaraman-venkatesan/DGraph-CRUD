# DGraph-CRUD
This repo is to understand and play with DGraph. Written in C#

Dependencies :
- FluentResults v2.15.2
- Google protobuf v3.24
- GRpc .net client 2.38.0. 
- Build the repo https://github.com/dgraph-io/dgraph.net from scratch and manually added the reference. This is done because recent changes after 21.0 is not added to nuget repo and its difficult to work with older version ( Strong suggesstion is to build from scratch and reference it)

Running Dgraph standalone version v21.0 

- Docker command to run :
> docker run -it -p 6080:6080 -p 8080:8080 -p 9080:9080 -p 8000:8000 -v /dgraph dgraph/standalone:v21.03.0
- For more info to run Dgraph standalone and its uses refer official documentation

- About the Solution
  - Used .NET and `C#`
  - Did the following operations on the standalone cluster
    - Version check of Dgraph connecting to
    - Schema creation
    - Insert data
    - Read by id
    - Update by condition
   
Few debugging points
-  Make sure to have proper dependencies
-  Make sure the client code is connecting to GRPC endpoint of the Dgraph. Any other endpoint will result in error
-  Make sure your code have `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);` for connecting to insecure or local standalone instance of Dgraph
