# DaprDemos

The purpose of these demos is to demonstarte using dapr capabilities from c#. Each example should contain only the minium required to demonstrate the concept.

# Prerequisites 


## Initial Demos
* [Install dapr](https://docs.dapr.io/getting-started/)
    * This requires docker as well
* [dotnet 5](https://dotnet.microsoft.com/download)

## Later Demos
* [Visual Studio Code](https://code.visualstudio.com/)
* Extensions:
    * C#
    * Dapr

 | Name | Description|
 |-|-|
 |[HelloWebApi](DaprDemo.D001.HelloWebApi)| Dapr invocation of an HTTP service |
 |[HelloGrpc](DaprDemo.D002.HelloGrpc) | Dapr invocation of a Grpc service|
 |[WebApiSubscriber](DaprDemo.D003.WebApiSubscriber) | HTTP Topic Subscriber|
 |[GrpcSubscriber](DaprDemo.D004.GrpcSubscriber) | Grpc Topic Subscriber|
 |[ReadSecrets](DaprDemo.D005.ReadSecrets) | Read from Dapr secret component using IConfigurationProvider |
