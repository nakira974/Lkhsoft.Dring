#region

using UIKit;

#endregion

namespace Lkhsoft.Dring.Client.Services;

public partial class SipBackgroundService
{
    private nint _taskId;

    private partial void PlatformStart()
    {
        _sipServer.Start();
        _taskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
        {
            // Gestion de la fin du temps alloué en arrière-plan
            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
            _sipServer.Stop();
        });
    }

    private partial void PlatformStop()
    {
        _sipServer.Stop();
        UIApplication.SharedApplication.EndBackgroundTask(_taskId);
    }

    private partial Task PlatformExecuteAsync(CancellationToken stoppingToken) // Ajout du modificateur d'accès
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
                // Exécuter la logique du réseau P2P
                Thread.Sleep(1000);
        }, stoppingToken);
    }
}