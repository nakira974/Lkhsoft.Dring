using Lkhsoft.Dring.Client.Services.Multimedia;

namespace Lkhsoft.Dring.Client.Models;

public class VideoDevice
{
    /// <summary>
    /// Device index
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Device name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public VideoDevice(HostVideoDevice videoDevice)
    {
        Index = videoDevice.Index;
        Name = videoDevice.Name;
    }
}