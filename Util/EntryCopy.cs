using System;
using System.Diagnostics;

namespace TPMuteMe.Util
{
    public static class EntryCopy
    {
        [Conditional("DEBUG")]
        public static void RefreshEntryFile()
        {
            if (!EntryFileChanged())
            {
                return;
            }

            KillTouchPortal();

            if (!Directory.Exists(PluginInfo.PluginDirectory))
            {
                Directory.CreateDirectory(PluginInfo.PluginDirectory);
            }

            if (File.Exists(PluginInfo.EntryTpPath))
            {
                File.Delete(PluginInfo.EntryTpPath);
            }

            File.Copy("entry.tp", PluginInfo.EntryTpPath);

            StartTouchPortal();
        }

        private static Boolean EntryFileChanged()
        {
            return !File.Exists(PluginInfo.EntryTpPath) ||
                   !File.ReadAllBytes("entry.tp").SequenceEqual(File.ReadAllBytes(PluginInfo.EntryTpPath));
        }

        private static void KillTouchPortal()
        {
            foreach (Process process in Process.GetProcessesByName("TouchPortal"))
            {
                process.Kill();
            }
        }

        private static void StartTouchPortal()
        {
            if (OperatingSystem.IsWindows())
            {
                String touchPortalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Touch Portal", "TouchPortal.exe");
                Process.Start(touchPortalPath);
                Thread.Sleep(10000);
            }
        }
    }
}
