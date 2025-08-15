using System;
using System.IO;

namespace WordCountToast
{
    internal static class StartupHelper
    {
        public static void AddToStartup(string appName, string exePath)
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, $"{appName}.url");

            string shortcutContent = $"[InternetShortcut]{Environment.NewLine}URL=file:///{exePath.Replace("\\", "/")}{Environment.NewLine}";
            File.WriteAllText(shortcutPath, shortcutContent);
        }

        public static void RemoveFromStartup(string appName)
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, $"{appName}.url");
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }
    }
}
