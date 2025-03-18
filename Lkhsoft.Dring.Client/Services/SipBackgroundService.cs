using Lkhsoft.Dring.Client.Services.SIP;
using Microsoft.Extensions.Hosting;

namespace Lkhsoft.Dring.Client.Services;

public partial class SipBackgroundService : BackgroundService
{
    private readonly SipServer _sipServer;
    
    public SipBackgroundService(SipServer sipServer)
    {
        _sipServer = sipServer;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Surveiller la connectivité réseau
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        try
        {
            PlatformStart();
            // Exécutez la logique principale du service
            while (!stoppingToken.IsCancellationRequested)
            {
                await PlatformExecuteAsync(stoppingToken); // Appel de la méthode partielle spécifique à la plateforme
                await Task.Delay(1000, stoppingToken); // Attendre avant la prochaine itération
            }
        }
        finally
        {
            // Arrêtez les services spécifiques à la plateforme
            PlatformStop();
        }

        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            Console.WriteLine("Réseau disponible.");
        }
        else
        {
            Console.WriteLine("Réseau indisponible.");
        }
    }

    private partial void PlatformStart();
    private partial void PlatformStop();
    private partial Task PlatformExecuteAsync(CancellationToken stoppingToken); // Ajout du modificateur d'accès
}