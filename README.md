# GrpcNamedPipes

.NET 6 client and server communicating with gRPC over named pipes.

## Server

1. Register `NamedPipeTransportFactory`
2. Configure Kestrel to listen to a named pipe endpoint.

```csharp
builder.Services.AddSingleton<IConnectionListenerFactory, NamedPipeTransportFactory>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(new NamedPipeEndPoint("Pipe-1"), listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

## Client

1. Create `SocketsHttpHandler` that uses `ConnectCallback` to establish named pipe with the server.
2. Create `GrpcChannel` using the handler.

```csharp
using var channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions
{
    HttpHandler = CreateHandler("Pipe-1")
});
```

## Run example

1. Start server: `dotnet run --project server`
2. Call server with client: `dotnet run --project client`
