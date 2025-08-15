using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WordCountToast
{
    public class PopupForm : Form
    {
        private readonly Label _label;
        private readonly Timer _timer;

        // tune these to taste
        private const int CornerRadius = 12;
        private const int EdgeMargin = 24;
        private const int AutoHideMs = 1800;
        private const int PaddingPx = 14;

        private Color _bg, _fg, _border;

        public PopupForm()
        {
            // non-activating, borderless, topmost
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;

            // Windows 11 dark mode + rounded corner hints (no-op on older builds)
            Theme.TryApplySystemDarkMode(this.Handle);
            Theme.TryApplyRoundedCorners(this.Handle);

            // Choose colors based on system preference
            bool light = Theme.IsAppLightTheme();
            _bg = light ? Color.FromArgb(245, 245, 245) : Color.FromArgb(32, 32, 32);
            _fg = light ? Color.FromArgb(12, 12, 12) : Color.FromArgb(234, 234, 234);
            _border = light ? Color.FromArgb(210, 210, 210) : Color.FromArgb(58, 58, 58);

            string fontName = Theme.GetSegoeUIFontName();
            FontFamily ff;
            try
            {
                ff = new FontFamily(fontName);
            }
            catch
            {
                ff = FontFamily.GenericSansSerif;
            }

            _label = new Label
            {
                AutoSize = true,
                ForeColor = _fg,
                BackColor = Color.Transparent,
                Font = new Font(ff, 11f, FontStyle.Bold, GraphicsUnit.Point)
            };


            Padding = new Padding(PaddingPx);
            Controls.Add(_label);

            _timer = new Timer { Interval = AutoHideMs };
            _timer.Tick += (_, __) => Hide();

            // Reduce flicker and make GDI+ edges cleaner
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        // keep it from stealing focus
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }
        protected override bool ShowWithoutActivation => true;

        public void ShowCount(int words)
        {
            _label.Text = $"{words} word" + (words == 1 ? "" : "s");

            _label.Location = new Point(Padding.Left, Padding.Top);
            Size = new Size(_label.PreferredWidth + Padding.Horizontal,
                            _label.PreferredHeight + Padding.Vertical);

            Region = new Region(CreateRoundRect(ClientRectangle, CornerRadius));

            var wa = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(Point.Empty);
            Location = new Point(wa.Right - Width - EdgeMargin, wa.Bottom - Height - EdgeMargin);

            _timer.Stop();
            Show();
            _timer.Start();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // smooth edges
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            using var path = CreateRoundRect(rect, CornerRadius);
            using var fill = new SolidBrush(_bg);
            using var pen = new Pen(_border, 1);

            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundRect(Rectangle bounds, int r)
        {
            int d = r * 2;
            var gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            gp.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            gp.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            gp.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
