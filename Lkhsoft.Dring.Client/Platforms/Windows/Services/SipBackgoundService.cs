using Microsoft.Extensions.Hosting;

namespace Lkhsoft.Dring.Client.Services;

public partial class SipBackgroundService : BackgroundService
{
    private partial void PlatformStart()
    {
        _sipServer.Start();
    }

    private partial void PlatformStop()
    {
        _sipServer.Stop();
    }
    
    private partial Task PlatformExecuteAsync(CancellationToken stoppingToken) // Ajout du modificateur d'accès
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Exécuter la logique du réseau P2P
                Thread.Sleep(1000);
            }
        }, stoppingToken);
    }
}