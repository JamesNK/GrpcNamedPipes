using System.Net;
using System.Net.Security;
using System.Security.Principal;
using Client;
using Greet;
using Grpc.Net.Client;

AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

Console.WriteLine("Client identity: " + Thread.CurrentPrincipal?.Identity?.Name);

try
{
    await MakeGrpcRequest();

    await MakeHttpRequest();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("Press any key to exit");
Console.ReadKey();

static async Task MakeGrpcRequest()
{
    Console.WriteLine("Making gRPC request...");
    using var channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions
    {
        HttpHandler = CreateHandler("Pipe-1", impersonationLevel: TokenImpersonationLevel.Impersonation)
    });
    var client = new Greeter.GreeterClient(channel);

    var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
    Console.WriteLine("Greeting: " + reply.Message);
}

static async Task MakeHttpRequest()
{
    Console.WriteLine("Making HTTP request...");
    using var client = new HttpClient(CreateHandler("Pipe-1", impersonationLevel: TokenImpersonationLevel.Impersonation));

    var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1/");
    request.Version = HttpVersion.Version20;
    request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    Console.WriteLine("HTTP version: " + response.Version);
    Console.WriteLine("Server identity: " + string.Join(",", response.Headers.GetValues("X-Server-Identity")));
    Console.WriteLine("Impersonated identity: " + string.Join(",", response.Headers.GetValues("X-Impersonated-Identity")));
}

static SocketsHttpHandler CreateHandler(string pipeName, TokenImpersonationLevel? impersonationLevel = null)
{
    var httpHandler = new SocketsHttpHandler();
    httpHandler.SslOptions = new SslClientAuthenticationOptions
    {
        RemoteCertificateValidationCallback = (_, __, ___, ____) => true
    };

    var connectionFactory = new NamedPipesConnectionFactory(pipeName, impersonationLevel);
    httpHandler.ConnectCallback = connectionFactory.ConnectAsync;

    return httpHandler;
}
