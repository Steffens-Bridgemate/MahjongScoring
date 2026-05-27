using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Tsump.QrScanner;

internal sealed class MainForm : Form
{
    private readonly Label _statusLabel;
    private readonly Label _urlLabel;
    private readonly TextBox _scanInput;

    private static readonly Color ColorIdle    = Color.FromArgb(160, 160, 160);
    private static readonly Color ColorOk      = Color.FromArgb(80,  210, 80);
    private static readonly Color ColorError   = Color.FromArgb(210, 70,  70);
    private static readonly Color ColorBg      = Color.FromArgb(28,  28,  28);
    private static readonly Color ColorInputBg = Color.FromArgb(42,  42,  42);

    public MainForm()
    {
        Text            = "Tsumo! QR Scanner";
        ClientSize      = new Size(420, 110);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        TopMost         = true;
        BackColor       = ColorBg;

        _statusLabel = new Label
        {
            Text      = "Scan een QR-code...",
            AutoSize  = false,
            Bounds    = new Rectangle(12, 8, 396, 36),
            Font      = new Font("Segoe UI", 13f),
            ForeColor = ColorIdle,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        _urlLabel = new Label
        {
            Text      = "",
            AutoSize  = false,
            Bounds    = new Rectangle(12, 46, 396, 18),
            Font      = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(80, 80, 80),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        // Visible input field — gives feedback that the scanner typed something,
        // and lets the operator type/paste a URL manually if needed.
        _scanInput = new TextBox
        {
            Bounds          = new Rectangle(12, 74, 396, 22),
            Font            = new Font("Segoe UI", 8f),
            BackColor       = ColorInputBg,
            ForeColor       = Color.FromArgb(140, 140, 140),
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "Wacht op scanner…",
        };
        _scanInput.KeyDown += OnKeyDown;

        Controls.Add(_statusLabel);
        Controls.Add(_urlLabel);
        Controls.Add(_scanInput);

        // Re-focus the input whenever the window comes to the front, so the
        // scanner (which acts as a keyboard) always sends its characters here.
        Activated          += (_, _) => _scanInput.Focus();
        _statusLabel.Click += (_, _) => _scanInput.Focus();
        _urlLabel.Click    += (_, _) => _scanInput.Focus();
        Click              += (_, _) => _scanInput.Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is not (Keys.Enter or Keys.Tab)) return;
        e.SuppressKeyPress = true;

        var url = _scanInput.Text.Trim();
        _scanInput.Clear();

        if (string.IsNullOrEmpty(url)) return;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && uri.Scheme is "https" or "http")
        {
            Clipboard.SetText(url);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            BeginInvoke(Activate);  // re-grab focus so the next scan still lands here
            ShowStatus("✓  Geopend in browser", ColorOk, url);
        }
        else
        {
            ShowStatus("✗  Geen geldige URL", ColorError, url);
        }
    }

    private System.Windows.Forms.Timer? _resetTimer;

    private void ShowStatus(string message, Color color, string url)
    {
        _statusLabel.Text     = message;
        _statusLabel.ForeColor = color;
        _urlLabel.Text        = url.Length > 68 ? url[..68] + "…" : url;
        _urlLabel.ForeColor   = color == ColorOk
            ? Color.FromArgb(60, 120, 60)
            : Color.FromArgb(120, 60, 60);

        _resetTimer?.Stop();
        _resetTimer?.Dispose();
        _resetTimer = new System.Windows.Forms.Timer { Interval = 2500 };
        _resetTimer.Tick += (_, _) =>
        {
            _resetTimer.Stop();
            _resetTimer.Dispose();
            _resetTimer = null;
            _statusLabel.Text      = "Scan een QR-code...";
            _statusLabel.ForeColor = ColorIdle;
        };
        _resetTimer.Start();
    }
}
