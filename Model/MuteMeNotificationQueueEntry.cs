using TPMuteMe.Enum;

namespace TPMuteMe.Model;

/// <summary>
/// Model for queueing MuteMe changes
/// </summary>
public class MuteMeNotificationQueueEntry
{
    /// <summary>
    /// The next color
    /// </summary>
    public MuteMeColor Color { get; set; }
    
    /// <summary>
    /// The next mode
    /// </summary>
    public MuteMeMode Mode { get; set; }

    /// <summary>
    /// Wait time in ms to wait after the change
    /// </summary>
    public UInt32 Delay { get; set; }
}
