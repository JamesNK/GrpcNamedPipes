using System.IO.Pipes;
using System.Security.Principal;

namespace Client;

public class NamedPipesConnectionFactory
{
    private readonly string _pipeName;
    private readonly TokenImpersonationLevel? _impersonationLevel;

    public NamedPipesConnectionFactory(string pipeName, TokenImpersonationLevel? impersonationLevel = null)
    {
        _pipeName = pipeName;
        _impersonationLevel = impersonationLevel;
    }

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
        CancellationToken cancellationToken = default)
    {
        var clientStream = new NamedPipeClientStream(
            serverName: ".",
            pipeName: _pipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
            impersonationLevel: _impersonationLevel ?? TokenImpersonationLevel.Anonymous);

        try
        {
            await clientStream.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return clientStream;
        }
        catch
        {
            clientStream.Dispose();
            throw;
        }
    }
}