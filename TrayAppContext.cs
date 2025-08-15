using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordCountToast
{
    public class TrayAppContext : ApplicationContext
    {
        // P/Invoke for clipboard listener
        [DllImport("user32.dll")] private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly NotifyIcon _tray;
        private readonly PopupForm _popup;
        private readonly HiddenForm _hiddenForm;

        public TrayAppContext()
        {
            // hidden message-only form for clipboard events
            _hiddenForm = new HiddenForm();
            _hiddenForm.ClipboardUpdate += OnClipboardUpdated;
            _hiddenForm.Show();

            _tray = new NotifyIcon
            {
                Visible = true,
                Text = "Word Counter",
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (_, __) => ExitThread());
            _tray.ContextMenuStrip = menu;

            _popup = new PopupForm();

            StartupHelper.AddToStartup("WordCountToast", Application.ExecutablePath);
        }

        private async void OnClipboardUpdated()
        {
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
                catch { /* clipboard busy */ }
                await Task.Delay(delayMs);
            }
            return null;
        }

        private static int CountWords(string input)
        {
            var matches = Regex.Matches(input, @"[\p{L}\p{N}][\p{L}\p{N}\p{Mn}\p{Pd}'’]*");
            return matches.Count;
        }

        protected override void ExitThreadCore()
        {
            _tray.Visible = false;
            _hiddenForm.Dispose();
            base.ExitThreadCore();
        }

        private class HiddenForm : Form
        {
            public event Action? ClipboardUpdate;

            public HiddenForm()
            {
                ShowInTaskbar = false;
                WindowState = FormWindowState.Minimized;
                Opacity = 0;
                FormBorderStyle = FormBorderStyle.None;
                Size = new Size(0, 0);
                StartPosition = FormStartPosition.Manual;
                Location = new Point(-10000, -10000);
                TopMost = false;
                ControlBox = false;
                MinimizeBox = false;
                MaximizeBox = false;
                Enabled = false;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_TOOLWINDOW = 0x80;
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                AddClipboardFormatListener(Handle);
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                RemoveClipboardFormatListener(Handle);
                base.OnHandleDestroyed(e);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    ClipboardUpdate?.Invoke();
                }
                base.WndProc(ref m);
            }
        }
    }
}
