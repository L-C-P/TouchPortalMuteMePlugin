using Microsoft.Extensions.Logging;
using TouchPortalSDK.Interfaces;
using TouchPortalSDK.Messages.Events;
using TPMuteMe.Enum;
using TPMuteMe.Util;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace TPMuteMe;

public class MuteMePlugin : PluginBase
{
    private const String CPluginId = "info.sowa.muteme";
    private const String CSetColorModeId = "info.sowa.muteme.action.set";
    private const String CSignalId = "info.sowa.muteme.action.signal";
    private const String CNotificationId = "info.sowa.muteme.action.notification";
    private const String CSetColorId = "info.sowa.muteme.action.set.color";
    private const String CSetColor2Id = "info.sowa.muteme.action.set.color2";
    private const String CSetModeId = "info.sowa.muteme.action.set.mode";
    private const String CSetSignalModeId = "info.sowa.muteme.action.set.signalmode";
    private const String CSetNotificationModeId = "info.sowa.muteme.action.set.notificationmode";
    private const String CSetNotificationDelayId = "info.sowa.muteme.action.set.notificationdelay";

    private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
    private readonly MuteMe _MuteMe;

    public MuteMePlugin(ITouchPortalClientFactory clientFactory, MuteMe muteMe, ILogger<MuteMePlugin> logger)
    {
        _MuteMe = muteMe;
        Logger = logger;
        Client = clientFactory.Create(this);
    }

    protected override ILogger Logger { get; }

    protected override ITouchPortalClient Client { get; }

    internal override String GetPluginId()
    {
        return CPluginId;
    }

    public override void OnActionEvent(ActionEvent message)
    {
        try
        {
            if (message.PluginId != CPluginId)
            {
                return;
            }

            switch (message.Type)
            {
                case "action":
                    switch (message.ActionId)
                    {
                        case CSetColorModeId:
                            {
                                String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

                                if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing color {strColor}");
                                    break;
                                }

                                String? strMode = message.Data.FirstOrDefault(d => d.Id == CSetModeId)?.Value.Replace(" ", String.Empty);

                                if (strMode == null || !System.Enum.TryParse(typeof(MuteMeMode), strMode, out Object? mode) || mode == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing mode \"{strMode}\"");
                                    break;
                                }

                                Logger.LogInformation($"MuteMe set color to \"{strColor}\" and mode to \"{strMode}\"");
                                _MuteMe.SetColorAndMode((MuteMeColor)color, (MuteMeMode)mode);
                            }
                            break;
                        case CSignalId:
                            {
                                String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

                                if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing color {strColor}");
                                    break;
                                }

                                String? strColor2 = message.Data.FirstOrDefault(d => d.Id == CSetColor2Id)?.Value.Replace(" ", String.Empty);

                                if (strColor2 == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor2, out Object? color2) || color2 == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing color2 {strColor2}");
                                    break;
                                }

                                String? strSignalMode = message.Data.FirstOrDefault(d => d.Id == CSetSignalModeId)?.Value.Replace(" ", String.Empty);

                                if (strSignalMode == null || !System.Enum.TryParse(typeof(MuteMeSignalMode), strSignalMode, out Object? signalMode) || signalMode == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing signal mode {strSignalMode}");
                                    break;
                                }

                                Logger.LogInformation($"MuteMe signal \"{strColor}\" - \"{strColor2}\" with mode \"{strSignalMode}\"");
                                _MuteMe.Signal((MuteMeColor)color, (MuteMeColor)color2, (MuteMeSignalMode)signalMode);
                            }
                            break;
                        case CNotificationId:
                            {
                                String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

                                if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing color {strColor}");
                                    break;
                                }

                                String? strNotificationMode = message.Data.FirstOrDefault(d => d.Id == CSetNotificationModeId)?.Value.Replace(" ", String.Empty);

                                if (strNotificationMode == null || !System.Enum.TryParse(typeof(MuteMeNotificationMode), strNotificationMode, out Object? notificationMode) || notificationMode == null)
                                {
                                    Logger.LogError($"MuteMe: Error parsing notification mode \"{strNotificationMode}\"");
                                    break;
                                }

                                String? strNotificationDelay = message.Data.FirstOrDefault(d => d.Id == CSetNotificationDelayId)?.Value.Replace(" ", String.Empty);

                                if (strNotificationDelay == null || !UInt32.TryParse(strNotificationDelay, out UInt32 notificationDelay))
                                {
                                    Logger.LogError($"MuteMe: Error parsing delay \"{strNotificationDelay}\"");
                                    break;
                                }

                                Logger.LogInformation($"MuteMe notification \"{strColor}\" with mode \"{strNotificationMode}\" and \"{notificationDelay}\"s delay");
                                _MuteMe.Notification((MuteMeColor)color, (MuteMeNotificationMode)notificationMode, notificationDelay);
                            }
                            break;
                    }

                    break;
                case "up":
                    break;
                case "down":
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"MuteMe OnActionEvent: {ex.Message}");
        }
    }

    public void Run()
    {
        try
        {
            if (!Client.Connect())
            {
                Logger.LogError("MuteMe-Client could not connect");

                return;
            }

            Logger.LogInformation("MuteMe-Client connected");

            //Client.ShowNotification($"info.sowa.muteme|update", "MuteMe: new version", "Please update to version 1.0!", new[] {
            //    new NotificationOptions()
            //    {
            //        Id = "update",
            //        Title = "Update this plugin"
            //    },
            //    new NotificationOptions()
            //    {
            //        Id = "readMore",
            //        Title = "Read more..."
            //    } });

            _MuteMe.Connect(Client, _CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Logger.LogError($"MuteMe Run: {ex.Message}");
        }
    }

    public override void OnClosedEvent(String message)
    {
        try
        {
            _CancellationTokenSource.Cancel();
        }
        finally
        {
            Environment.Exit(0);
        }
    }
}
