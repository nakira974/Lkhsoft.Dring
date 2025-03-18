namespace Lkhsoft.Dring.Client.Models.P2P;

/// <summary>
/// Peer node information
/// </summary>
public class NodeInfo
{
    /// <summary>
    /// Peer id
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Node address
    /// </summary>
    public string Address { get; set; }
    /// <summary>
    /// Node port
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// Node metric
    /// </summary>
    public int Metric { get; set; }
}