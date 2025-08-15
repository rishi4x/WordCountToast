using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordCountToast
{
    public partial class MainForm : Form
    {
        // P/Invoke for clipboard listener
        [DllImport("user32.dll")] private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly NotifyIcon _tray;
        private readonly PopupForm _popup;

        public MainForm()
        {
            InitializeComponent();
            //SetStartupShortcut(true);

            // keep the main form hidden/minimized
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Opacity = 0;

            // tray icon
            _tray = new NotifyIcon
            {
                Visible = true,
                Text = "Word Counter",
                Icon = System.Drawing.SystemIcons.Information
            };

            _tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

            var menu = new ContextMenuStrip();
            var exit = new ToolStripMenuItem("Exit", null, (_, __) => Close());
            menu.Items.Add(exit);
            _tray.ContextMenuStrip = menu;

            _popup = new PopupForm();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode) AddClipboardFormatListener(Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!DesignMode) RemoveClipboardFormatListener(Handle);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardUpdated();
            }
            base.WndProc(ref m);
        }

        private async void OnClipboardUpdated()
        {
            // try read text
            string? text = await TryGetClipboardTextAsync();
            if (string.IsNullOrWhiteSpace(text)) return;

            int words = CountWords(text);
            _popup.ShowCount(words);
        }

        private static async Task<string?> TryGetClipboardTextAsync(int retries = 3, int delayMs = 60)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (Clipboard.ContainsText())
                        return Clipboard.GetText(TextDataFormat.UnicodeText);
                }
                catch { /* clipboard busy, retry */ }
                await Task.Delay(delayMs);
            }
            return null;
        }

        private static int CountWords(string input)
        {
            // reasonable word regex
            var matches = Regex.Matches(input, @"\p{L}[\p{L}\p{Mn}\p{Pd}'’]*");
            return matches.Count;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add this app to startup on first run
            StartupHelper.AddToStartup("WordCountToast", Application.ExecutablePath);
        }

        //private void SetStartupShortcut(bool enable)
        //{
        //    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        //    string shortcutPath = Path.Combine(startupFolder, "WordCountToast.lnk");
        //    string exePath = Application.ExecutablePath;

        //    if (enable)
        //    {
        //        var shell = new WshShell();
        //        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        //        shortcut.TargetPath = exePath;
        //        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
        //        shortcut.Save();
        //    }
        //    else
        //    {
        //        if (System.IO.File.Exists(shortcutPath))
        //            System.IO.File.Delete(shortcutPath);
        //    }
        //}

    }
}
