using Microsoft.Extensions.Hosting;
using UIKit;

namespace Lkhsoft.Dring.Client.Services;

public partial class SipBackgroundService
{
    private nint _taskId;

    private partial void PlatformStart()
    {
        Console.WriteLine("Démarrage des services spécifiques à macCatalyst...");
        _sipServer.Start();
        // Commencer une tâche en arrière-plan
        _taskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
        {
            Console.WriteLine("Tâche en arrière-plan terminée.");
            _sipServer.Stop();
            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        });
    }

    private partial void PlatformStop()
    {
        Console.WriteLine("Arrêt des services spécifiques à macCatalyst...");

        // Terminer la tâche en arrière-plan
        _sipServer.Stop();
        UIApplication.SharedApplication.EndBackgroundTask(_taskId);
    }

    private partial Task PlatformExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            Console.WriteLine("Exécution de la logique spécifique à macCatalyst...");
            while (!stoppingToken.IsCancellationRequested)
            {
                // Logique du réseau P2P
                Thread.Sleep(1000);
            }
        }, stoppingToken);
    }
}