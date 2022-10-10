using System.Security.Principal;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes.Internal;
using Server;

AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddSingleton<IConnectionListenerFactory, NamedPipeTransportFactory>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(new NamedPipeEndPoint("Pipe-1"), listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.MapGet("/", async (context) =>
{
    var serverName = Thread.CurrentPrincipal!.Identity!.Name;

    var namedPipeStream = context.Features.Get<IConnectionNamedPipeFeature>()!.NamedPipe;
    var impersonatedName = namedPipeStream.GetImpersonationUserName();

    context.Response.Headers.Add("X-Server-Identity", serverName);
    context.Response.Headers.Add("X-Impersonated-Identity", impersonatedName);

    await context.Response.WriteAsync("hello, world");
});
app.MapGrpcService<GreeterService>();

app.Run();
