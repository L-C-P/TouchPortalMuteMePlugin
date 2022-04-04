using Microsoft.Extensions.Logging;
using TouchPortalSDK.Interfaces;
using TouchPortalSDK.Messages.Events;
using TPMuteMe.Enum;
using TPMuteMe.Util;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace TPMuteMe;

/// <summary>
/// The Touch Portal plugin implementation.
/// </summary>
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

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="clientFactory">Touch Portal client factory.</param>
    /// <param name="muteMe">MuteMe instance.</param>
    /// <param name="logger">The logger.</param>
    public MuteMePlugin(ITouchPortalClientFactory clientFactory, MuteMe muteMe, ILogger<MuteMePlugin> logger)
    {
        _MuteMe = muteMe;
        Logger = logger;
        Client = clientFactory.Create(this);
    }

    /// <summary>
    /// The logger.
    /// </summary>
    protected override ILogger Logger { get; }

    /// <summary>
    /// Touch Portal client.
    /// </summary>
    protected override ITouchPortalClient Client { get; }

    /// <summary>
    /// Set the plugin id for the base class.
    /// </summary>
    /// <returns>The id.</returns>
    protected override String GetPluginId()
    {
        return CPluginId;
    }

    /// <summary>
    /// Touch Portal signals an action.
    /// </summary>
    /// <param name="message">The content (parameters) of the action.</param>
    public override void OnActionEvent(ActionEvent message)
    {
        try
        {
            // Is the message for me?
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
                                SetMuteMeColorAndMode(message);
                            }
                            break;
                        case CSignalId:
                            {
                                SignalMuteMe(message);
                            }
                            break;
                        case CNotificationId:
                            {
                                SetMuteMeNotification(message);
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

    /// <summary>
    /// Set MuteMe notification.
    /// </summary>
    /// <param name="message">The parameters for the action.</param>
    private void SetMuteMeNotification(ActionEvent message)
    {
        String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

        if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
        {
            Logger.LogError($"MuteMe: Error parsing color {strColor}");
            return;
        }

        String? strNotificationMode = message.Data.FirstOrDefault(d => d.Id == CSetNotificationModeId)?.Value.Replace(" ", String.Empty);

        if (strNotificationMode == null || !System.Enum.TryParse(typeof(MuteMeNotificationMode), strNotificationMode, out Object? notificationMode) || notificationMode == null)
        {
            Logger.LogError($"MuteMe: Error parsing notification mode \"{strNotificationMode}\"");
            return;
        }

        String? strNotificationDelay = message.Data.FirstOrDefault(d => d.Id == CSetNotificationDelayId)?.Value.Replace(" ", String.Empty);

        if (strNotificationDelay == null || !UInt32.TryParse(strNotificationDelay, out UInt32 notificationDelay))
        {
            Logger.LogError($"MuteMe: Error parsing delay \"{strNotificationDelay}\"");
            return;
        }

        Logger.LogInformation($"MuteMe notification \"{strColor}\" with mode \"{strNotificationMode}\" and \"{notificationDelay}\"s delay");
        _MuteMe.Notification((MuteMeColor) color, (MuteMeNotificationMode) notificationMode, notificationDelay);
    }

    /// <summary>
    /// Signal MuteM
    /// </summary>
    /// <param name="message">The parameters for the action.</param>
    private void SignalMuteMe(ActionEvent message)
    {
        String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

        if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
        {
            Logger.LogError($"MuteMe: Error parsing color {strColor}");
            return;
        }

        String? strColor2 = message.Data.FirstOrDefault(d => d.Id == CSetColor2Id)?.Value.Replace(" ", String.Empty);

        if (strColor2 == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor2, out Object? color2) || color2 == null)
        {
            Logger.LogError($"MuteMe: Error parsing color2 {strColor2}");
            return;
        }

        String? strSignalMode = message.Data.FirstOrDefault(d => d.Id == CSetSignalModeId)?.Value.Replace(" ", String.Empty);

        if (strSignalMode == null || !System.Enum.TryParse(typeof(MuteMeSignalMode), strSignalMode, out Object? signalMode) || signalMode == null)
        {
            Logger.LogError($"MuteMe: Error parsing signal mode {strSignalMode}");
            return;
        }

        Logger.LogInformation($"MuteMe signal \"{strColor}\" - \"{strColor2}\" with mode \"{strSignalMode}\"");
        _MuteMe.Signal((MuteMeColor) color, (MuteMeColor) color2, (MuteMeSignalMode) signalMode);
    }

    /// <summary>
    /// Set MuteMe color and mode.
    /// </summary>
    /// <param name="message">The parameters for the action.</param>
    private void SetMuteMeColorAndMode(ActionEvent message)
    {
        String? strColor = message.Data.FirstOrDefault(d => d.Id == CSetColorId)?.Value.Replace(" ", String.Empty);

        if (strColor == null || !System.Enum.TryParse(typeof(MuteMeColor), strColor, out Object? color) || color == null)
        {
            Logger.LogError($"MuteMe: Error parsing color {strColor}");
            return;
        }

        String? strMode = message.Data.FirstOrDefault(d => d.Id == CSetModeId)?.Value.Replace(" ", String.Empty);

        if (strMode == null || !System.Enum.TryParse(typeof(MuteMeMode), strMode, out Object? mode) || mode == null)
        {
            Logger.LogError($"MuteMe: Error parsing mode \"{strMode}\"");
            return;
        }

        Logger.LogInformation($"MuteMe set color to \"{strColor}\" and mode to \"{strMode}\"");
        _MuteMe.SetColorAndMode((MuteMeColor) color, (MuteMeMode) mode);
    }

    /// <summary>
    /// PlugIn run. Connect to Touch Portal and MuteMe.
    /// </summary>
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

    /// <summary>
    /// Touch Portal signals, that the plugin is closed.
    /// </summary>
    /// <param name="message">The content of the event.</param>
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
