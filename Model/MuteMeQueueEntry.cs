using TPMuteMe.Enum;

namespace TPMuteMe.Model;

public class MuteMeQueueEntry
{
    public MuteMeColor Color { get; set; }
    public MuteMeMode Mode { get; set; }
    public UInt32 Delay { get; set; }
}
