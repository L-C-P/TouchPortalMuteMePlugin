using System.Reflection;

namespace TPMuteMe.Util
{
    public static class PluginInfo
    {
        public static String AssemblyName { get; }

        public static String TpPluginsDirectory { get; }

        public static String PluginDirectory { get; }

        public static String EntryTpPath { get; }

        static PluginInfo()
        {
            String myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            TpPluginsDirectory = Path.Combine(myDocuments, "TouchPortal", "plugins");

            if (!Directory.Exists(TpPluginsDirectory))
            {
                TpPluginsDirectory = Path.Combine(appDataRoaming, "TouchPortal", "plugins");
            }

            AssemblyName = Assembly.GetEntryAssembly()?.GetName().Name!;
            PluginDirectory = Path.Combine(TpPluginsDirectory, AssemblyName);
            EntryTpPath = Path.Combine(PluginDirectory, "entry.tp");
        }
    }
}
