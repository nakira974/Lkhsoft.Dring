using System.Collections.Concurrent;

namespace Lkhsoft.Dring.Client.Models.P2P;

public static class RoutingTable
{
    private static readonly ConcurrentDictionary<string, NodeInfo> _nodes = new ConcurrentDictionary<string, NodeInfo>();

    public static void AddOrUpdateNode(string nodeId, NodeInfo info)
    {
        _nodes.AddOrUpdate(nodeId, info, (key, oldValue) => info);
    }

    public static NodeInfo GetNextNode(string destinationUri)
    {
        // Logique de routage : ici, on retourne simplement le premier nœud disponible
        return _nodes.Values.FirstOrDefault();
    }
}
