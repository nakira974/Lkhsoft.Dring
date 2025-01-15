using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lkhsoft.Dring.Server.Utility.Core;

/// <summary>
///     Text writer that can write to a socket or the console
/// </summary>
public class ServerTextWriter : TextWriter
{
    /// <summary>
    ///     Original console output
    /// </summary>
    private readonly TextWriter _originalConsoleOut;
    
    /// <summary>
    ///     Sockets to write to
    /// </summary>
    private readonly ConcurrentDictionary<int, (UdpClient socket, IPEndPoint endPoint)> _sockets;
    
    /// <summary>
    ///   Encoding to use
    /// </summary>
    private readonly Encoding _encoding;
    
    /// <inheritdoc />
    public override Encoding Encoding => _encoding;

    /// <summary>
    ///     Basic constructor
    /// </summary>
    /// <param name="originalConsoleOut">Original console output to write in</param>
    /// <param name="encoding">Encoding used to write</param>
    public ServerTextWriter(TextWriter originalConsoleOut, Encoding encoding = null)
    {
        _originalConsoleOut = originalConsoleOut;
        _sockets = new ConcurrentDictionary<int, (UdpClient, IPEndPoint)>();
        _encoding = encoding ?? Encoding.UTF8;
    }

    /// <summary>
    ///     Add a socket to write to
    /// </summary>
    /// <param name="taskId">Task id</param>
    /// <param name="socket">Socket to write to</param>
    /// <param name="endPoint"></param>
    public void AddSocket(int taskId, UdpClient socket, IPEndPoint endPoint)
    {
        _sockets.TryAdd(taskId, (socket, endPoint));
    }

    /// <summary>
    ///     Remove a socket
    /// </summary>
    /// <param name="taskId"></param>
    public void RemoveSocket(int taskId)
    {
        _sockets.TryRemove(taskId, out _);
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<char> buffer)
    {
        var data = ConvertToBytes(buffer);
        SendToUdpClientOrConsole(data);
    }

    /// <inheritdoc />
    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        var data = ConvertToBytes(buffer);
        SendToUdpClientOrConsole(data);
        SendToUdpClientOrConsole(Encoding.UTF8.GetBytes(Environment.NewLine));
    }

    /// <summary>
    ///   Send data to the UDP client or the console
    /// </summary>
    /// <param name="data"></param>
    private void SendToUdpClientOrConsole(ReadOnlySpan<byte> data)
    {
        var taskId = Task.CurrentId ?? 0;
        if (_sockets.TryGetValue(taskId, out var udpClientInfo))
        {
            var (udpClient, endPoint) = udpClientInfo;
            udpClient.Send(data, endPoint);
        }
        else
        {
            _originalConsoleOut.Write(Encoding.GetString(data.ToArray()));
        }
    }
    
    /// <summary>
    ///   Convert a char buffer to a byte buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private ReadOnlySpan<byte> ConvertToBytes(ReadOnlySpan<char> buffer)
    {
        int byteCount = _encoding.GetByteCount(buffer);
        var byteArray = new byte[byteCount];
        _encoding.GetBytes(buffer, byteArray);
        return byteArray;
    }
}