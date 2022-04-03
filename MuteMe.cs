using HidSharp;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TouchPortalSDK.Interfaces;
using TPMuteMe.Enum;
using TPMuteMe.Model;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace TPMuteMe;

public class MuteMe
{
    private const Int32 CVendorId = 8352;
    private const Int32 CProductId = 17114;
    private readonly Object _Lock = new Object();
    private readonly ILogger _Logger;
    private readonly ConcurrentQueue<MuteMeQueueEntry> _QueueEntry = new ConcurrentQueue<MuteMeQueueEntry>();
    private readonly Byte[] _ReadBuffer = new Byte[8];
    private CancellationToken _CancellationToken;
    private ITouchPortalClient? _Client;
    private Stream? _HidStream;
    private MuteMeColor _LastColor = MuteMeColor.NoColor;
    private MuteMeMode _LastMode = MuteMeMode.Dim;
    private HidDevice? _MuteMe;
    private MuteMeColor _NotificationColor;
    private UInt32 _NotificationDelay;
    private MuteMeNotificationMode _NotificationMode;
    private DateTime _NotificationNextTrigger;
    private Boolean _SequenceStarted;
    private Thread? _Thread;
    private Boolean _Touched;

    public MuteMe(ILogger<MuteMe> logger)
    {
        _Logger = logger;
        Connected = false;
    }

    public Boolean Connected { get; private set; }

    public static Boolean HardwareConnected()
    {
        DeviceList? usbList = DeviceList.Local;

        return usbList?.GetHidDeviceOrNull(CVendorId, CProductId) != null;
    }

    public void Connect(ITouchPortalClient client, CancellationToken cancellationToken)
    {
        _Client = client ?? throw new ArgumentNullException(nameof(client));
        _CancellationToken = cancellationToken;

        // ReSharper disable once UseObjectOrCollectionInitializer
        _Thread = new Thread(() => Do(_CancellationToken));
        _Thread.Priority = ThreadPriority.BelowNormal;
        _Thread.Start();
    }

    private void Do(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (ConnectMuteMe(cancellationToken))
                    {
                        DoQueue(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, ex.Message);
                }
            }

            SetMuteMe(MuteMeColor.NoColor, MuteMeMode.Dim);
            Connected = false;
            _HidStream?.Close();
            _HidStream?.Dispose();
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, ex.Message);
        }
    }

    private void DoQueue(CancellationToken cancellationToken)
    {
        if (_QueueEntry.TryDequeue(out MuteMeQueueEntry? result))
        {
            _SequenceStarted = true;
            SetMuteMe(result.Color, result.Mode);
            cancellationToken.WaitHandle.WaitOne((Int32)result.Delay);
        }
        else
        {
            if (_SequenceStarted)
            {
                SetMuteMe(_LastColor, _LastMode);
                _SequenceStarted = false;
            }

            cancellationToken.WaitHandle.WaitOne(250);
        }

        DoNotification();
    }

    private void DoNotification()
    {
        if (_NotificationMode == MuteMeNotificationMode.Off)
        {
            return;
        }

        if (DateTime.Now > _NotificationNextTrigger)
        {
            if (_NotificationColor == _LastColor)
            {
                _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.NoColor, Mode = MuteMeMode.Dim, Delay = 100});
            }

            switch (_NotificationMode)
            {
                case MuteMeNotificationMode.Once:
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = _NotificationColor, Mode = MuteMeMode.FullBright, Delay = 100});
                    break;
                case MuteMeNotificationMode.Twice:
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = _NotificationColor, Mode = MuteMeMode.FullBright, Delay = 100});
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.NoColor, Mode = MuteMeMode.Dim, Delay = 100});
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = _NotificationColor, Mode = MuteMeMode.FullBright, Delay = 100});
                    break;
            }

            if (_NotificationColor == _LastColor)
            {
                _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.NoColor, Mode = MuteMeMode.Dim, Delay = 100});
            }

            _NotificationNextTrigger = DateTime.Now.AddSeconds(_NotificationDelay);
        }
    }

    private Boolean ConnectMuteMe(CancellationToken cancellationToken)
    {
        DeviceList? usbList = DeviceList.Local;
        _MuteMe = usbList?.GetHidDeviceOrNull(CVendorId, CProductId);

        if (_MuteMe == null)
        {
            Connected = false;
            _Logger.LogError("MuteMe not connected");
            cancellationToken.WaitHandle.WaitOne(5000);

            return false;
        }

        if (Connected)
        {
            return true;
        }

        if (!_MuteMe.TryOpen(out HidStream hidStream))
        {
            Connected = false;
            _Logger.LogError("Could not establish connection to MuteMe");
            cancellationToken.WaitHandle.WaitOne(5000);

            return false;
        }

        _HidStream?.Dispose();
        _HidStream = Stream.Synchronized(hidStream);
        _HidStream.WriteTimeout = 100;
        _HidStream.ReadTimeout = 100;
        _ = _HidStream.BeginRead(_ReadBuffer, 0, 8, BytesReady, null);

        Connected = true;
        _Logger.LogError("MuteMe connected");
        SignalConnected();

        return true;
    }

    private void SignalConnected()
    {
        _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.Red, Mode = MuteMeMode.FullBright, Delay = 250});
        _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.Green, Mode = MuteMeMode.FullBright, Delay = 250});
        _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = MuteMeColor.Blue, Mode = MuteMeMode.FullBright, Delay = 250});
    }

    private void BytesReady(IAsyncResult ar)
    {
        lock (_Lock)
        {
            switch (_ReadBuffer[4])
            {
                case 1:
                    if (!_Touched)
                    {
                        _Touched = true;
                        _Client?.StateUpdate("MuteMe_State", "Touched");
                        // _Client?.SendCommand(StateUpdateCommand.CreateAndValidate("info.sowa.muteme.event.touch", "Touched"));
                        _Logger.LogInformation("Touched");
                    }

                    break;
                case 2:
                    if (_Touched)
                    {
                        _Touched = false;
                        _Client?.StateUpdate("MuteMe_State", "Untouched");
                        // _Client?.SendCommand(StateUpdateCommand.CreateAndValidate("info.sowa.muteme.event.touch", "Untouched"));
                        _Logger.LogInformation("Untouched");
                    }

                    break;
            }
        }

        if (!_CancellationToken.IsCancellationRequested)
        {
            _ = _HidStream?.BeginRead(_ReadBuffer, 0, 8, BytesReady, null);
        }
    }

    public void SetColorAndMode(MuteMeColor color, MuteMeMode mode)
    {
        _LastColor = color;
        _LastMode = mode;
        _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color, Mode = mode});
    }

    public void Signal(MuteMeColor color, MuteMeColor color2, MuteMeSignalMode signalMode)
    {
        if (color == MuteMeColor.CurrentColor)
        {
            color = _LastColor;
        }

        if (color2 == MuteMeColor.CurrentColor)
        {
            color2 = _LastColor;
        }

        switch (signalMode)
        {
            case MuteMeSignalMode.Fast:
                for (Int32 i = 0; i < 8; i++)
                {
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color, Mode = MuteMeMode.FullBright, Delay = 100});
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color2, Mode = MuteMeMode.FullBright, Delay = 100});
                }

                break;
            case MuteMeSignalMode.Slow:
                for (Int32 i = 0; i < 4; i++)
                {
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color, Mode = MuteMeMode.FullBright, Delay = 250});
                    _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color2, Mode = MuteMeMode.FullBright, Delay = 250});
                }

                break;
            case MuteMeSignalMode.Once:
                _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color, Mode = MuteMeMode.FullBright, Delay = 800});
                _QueueEntry.Enqueue(new MuteMeQueueEntry {Color = color2, Mode = MuteMeMode.FullBright, Delay = 800});
                break;
        }
    }

    public void Notification(MuteMeColor color, MuteMeNotificationMode notificationMode, UInt32 delay)
    {
        _NotificationMode = notificationMode;
        _NotificationColor = color;
        _NotificationDelay = delay;
        _NotificationNextTrigger = DateTime.Now.AddSeconds(1);
    }

    private void SetMuteMe(MuteMeColor color, MuteMeMode mode)
    {
        if (!Connected)
        {
            return;
        }

        try
        {
            Byte command = (Byte)((Byte)color + (Byte)mode);

            lock (_Lock)
            {
                _HidStream?.Write(new Byte[] {0, command}, 0, 2);
            }
        }
        catch (Exception ex)
        {
            Connected = false;

            _Logger.LogError(ex, ex.Message);
        }
    }
}
