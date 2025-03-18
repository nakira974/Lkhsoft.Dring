using System.Net;
using Lkhsoft.Dring.Client.Models.P2P;
using SIPSorcery.SIP;

namespace Lkhsoft.Dring.Client.Services.SIP;

public class SipServer
{
    private SIPTransport _sipTransport;

    public SipServer()
    {
        // Configuration du transport SIP
        _sipTransport = new SIPTransport();
        _sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, 5060))); // Écoute sur le port 5060 (UDP)
    }

    public void Start()
    {
        Console.WriteLine("Serveur SIP démarré...");
        _sipTransport.SIPTransportRequestReceived += OnSipRequestReceived; // Écoute des requêtes SIP
    }

    public void Stop()
    {
        _sipTransport.SIPTransportRequestReceived -= OnSipRequestReceived;
        _sipTransport.Shutdown();
        Console.WriteLine("Serveur SIP arrêté.");
    }

    private async Task OnSipRequestReceived(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
    {
        Console.WriteLine($"Requête SIP reçue : {sipRequest.Method} de {remoteEndPoint}");

        switch (sipRequest.Method)
        {
            case SIPMethodsEnum.INVITE:
                await HandleInviteRequest(sipRequest, remoteEndPoint);
                break;
            case SIPMethodsEnum.BYE:
                await HandleByeRequest(sipRequest, remoteEndPoint);
                break;
            default:
                Console.WriteLine($"Méthode non gérée : {sipRequest.Method}");
                break;
        }
    }

    private Task HandleInviteRequest(SIPRequest inviteRequest, SIPEndPoint remoteEndPoint)
    {
        Console.WriteLine("Traitement d'une requête INVITE...");

        // Extraire l'URI de destination
        var destinationUri = inviteRequest.URI.ToString();
        Console.WriteLine($"Destination : {destinationUri}");

        // Logique de routage : déterminer le prochain nœud ou établir l'appel directement
        var nextNode = RoutingTable.GetNextNode(destinationUri);
        if (nextNode is not null)
        {
            Console.WriteLine($"Routage de l'appel vers : {nextNode.Id}");
            RouteCallToNextNode(inviteRequest, nextNode);
        }
        else
        {
            Console.WriteLine("Destination trouvée, établissement de l'appel...");
            EstablishCall(inviteRequest, remoteEndPoint);
        }
        
        return Task.CompletedTask;
    }

    private Task HandleByeRequest(SIPRequest byeRequest, SIPEndPoint remoteEndPoint)
    {
        Console.WriteLine("Traitement d'une requête BYE...");
        // Logique pour terminer l'appel

        return Task.CompletedTask;
    }

    private void RouteCallToNextNode(SIPRequest inviteRequest, NodeInfo nextNode)
    {
        // Envoyer la requête INVITE au prochain nœud
        var nextNodeEndPoint = new SIPEndPoint(new IPEndPoint(IPAddress.Parse(nextNode.Address), nextNode.Port));
        var forwardedRequest = inviteRequest.Copy();
        forwardedRequest.URI = SIPURI.ParseSIPURI($"sip:{nextNode.Id}@{nextNode.Address}:{nextNode.Port}");

        _sipTransport.SendRequestAsync(nextNodeEndPoint, forwardedRequest);
    }

    private void EstablishCall(SIPRequest inviteRequest, SIPEndPoint remoteEndPoint)
    {
        // Accepter l'appel et établir la communication
        var okResponse = new SIPResponse(SIPResponseStatusCodesEnum.Ok, "OK", inviteRequest.SIPEncoding, inviteRequest.SIPEncoding);
        _sipTransport.SendResponseAsync(remoteEndPoint, okResponse);

        Console.WriteLine("Appel établi avec succès.");
    }
}