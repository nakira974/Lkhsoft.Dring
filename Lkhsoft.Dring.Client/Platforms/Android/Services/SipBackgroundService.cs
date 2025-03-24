using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Microsoft.Extensions.Hosting;

namespace Lkhsoft.Dring.Client.Services;

public partial class SipBackgroundService : BackgroundService
{
    private ForegroundService _foregroundService;
    private ConnectivityManager _connectivityManager;

    private partial void PlatformStart()
    {
        var context = Android.App.Application.Context;
        var intent = new Intent(context, typeof(ForegroundService));
        context.StartForegroundService(intent);

        // Initialiser le ConnectivityManager
        _connectivityManager = (ConnectivityManager) context.GetSystemService(Context.ConnectivityService);
        _sipServer.Start();
    }

    private partial void PlatformStop()
    {
        var context = Android.App.Application.Context;
        var intent = new Intent(context, typeof(ForegroundService));
        context.StopService(intent);
        _sipServer.Stop();
    }

    private partial Task PlatformExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Vérifier la connectivité réseau
                if (IsNetworkAvailable())
                {
                    // Exécuter la logique du réseau P2P
                    Console.WriteLine("Réseau disponible, exécution de la logique P2P...");
                }
                else
                {
                    Console.WriteLine("Aucun réseau disponible, attente...");
                }

                Thread.Sleep(1000);
            }
        }, stoppingToken);
    }

    private bool IsNetworkAvailable()
    {
        if (_connectivityManager == null)
            return false;

        // Vérifier la connectivité réseau
        var activeNetwork = _connectivityManager.ActiveNetworkInfo;
        return activeNetwork != null && activeNetwork.IsConnectedOrConnecting;
    }

    [Service]
    public class ForegroundService : Service
    {
        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var notification = new Notification.Builder(this, "sip_service_channel")
                .SetContentTitle("Service SIP")
                .SetContentText("Service en cours d'exécution...")
                .SetSmallIcon(Resource.Drawable.ic_call_answer)
                .Build();

            StartForeground(1, notification);
            return StartCommandResult.Sticky;
        }
    }
}