using Microsoft.Extensions.Logging;
using TouchPortalSDK;
using TouchPortalSDK.Interfaces;
using TouchPortalSDK.Messages.Events;
using TouchPortalSDK.Messages.Models;

namespace TPMuteMe.Util
{
    /// <summary>
    /// Base class for MuteMe plugins
    /// </summary>
    public abstract class PluginBase : ITouchPortalEventHandler
    {
        /// <summary>
        /// The plugin id (unique).
        /// Use the same value in entry.tp.
        /// </summary>
        public String PluginId => GetPluginId();

        /// <summary>
        /// The logger
        /// </summary>
        protected abstract ILogger Logger { get; }

        /// <summary>
        /// The Touch Portal client.
        /// </summary>
        protected abstract ITouchPortalClient Client { get; }

        /// <summary>
        /// Get the pluginid.
        /// </summary>
        /// <returns>The id.</returns>
        protected abstract String GetPluginId();

        /// <summary>
        /// Touch Portal send an action event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnActionEvent(ActionEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an broadcast event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnBroadcastEvent(BroadcastEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an closed event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnClosedEvent(String message)
        {
            Logger.LogObjectAsJson(message);

            //Optional force exits this plugin.
            Environment.Exit(0);
        }

        /// <summary>
        /// Touch Portal connection state changed.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnConnecterChangeEvent(ConnectorChangeEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an info event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnInfoEvent(InfoEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an listchanged event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnListChangedEvent(ListChangeEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an notificationoptionclicked event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnNotificationOptionClickedEvent(NotificationOptionClickedEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an settings (changed) event.
        /// </summary>
        /// <param name="message">Content of the event.</param>
        public virtual void OnSettingsEvent(SettingsEvent message)
        {
            Logger.LogObjectAsJson(message);
        }

        /// <summary>
        /// Touch Portal send an shortconnectoridnotification event.
        /// </summary>
        /// <param name="connectorInfo">Content of the event.</param>
        public virtual void OnShortConnectorIdNotificationEvent(ConnectorInfo connectorInfo)
        {
            Logger.LogObjectAsJson(connectorInfo);
        }

        /// <summary>
        /// An unhandled event occured.
        /// </summary>
        /// <param name="jsonMessage">Content of the event.</param>
        public virtual void OnUnhandledEvent(String jsonMessage)
        {
            Logger.LogObjectAsJson(jsonMessage);
        }
    }
}
