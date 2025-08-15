using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace WordCountToast
{
    internal static class Theme
    {
        // DWM attributes
        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        }
        private enum DWM_WINDOW_CORNER_PREFERENCE
        {
            Default = 0,
            DoNotRound = 1,
            Round = 2,
            RoundSmall = 3
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        public static void TryApplyRoundedCorners(IntPtr hwnd)
        {
            try
            {
                int val = (int)DWM_WINDOW_CORNER_PREFERENCE.Round;
                DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                                      ref val, sizeof(int));
            }
            catch
            {
                // ignore
            }
        }

        public static void TryApplySystemDarkMode(IntPtr hwnd)
        {
            try
            {
                int dark = IsAppLightTheme() ? 0 : 1;
                DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                                      ref dark, sizeof(int));
            }
            catch
            {
                // ignore
            }
        }

        public static bool IsAppLightTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var v = key?.GetValue("AppsUseLightTheme");
                if (v is int i) return i != 0;
            }
            catch { }
            return true;
        }

        public static string GetSegoeUIFontName()
        {
            try
            {
                using var fonts = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
                if (fonts?.GetValue("Segoe UI Variable (TrueType)") != null)
                    return "Segoe UI Variable Display";
            }
            catch { }
            return "Segoe UI";
        }
    }
}
