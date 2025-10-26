using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace MiNavegador
{
    public class MainForm : Form
    {
        private ToolStrip ts;
        private ToolStripButton btnBack, btnForward, btnRefresh, btnStop, btnHome, btnFavAdd, btnFind, btnZoomOut, btnZoomReset, btnZoomIn;
        private ToolStripLabel lblLock;
        private ToolStripComboBox cboSearch;
        private ToolStripTextBox txtAddress, txtFind;
        private ToolStripDropDownButton ddFavs, ddMenu;
        private StatusStrip status;
        private ToolStripStatusLabel lblStatus, lblProgress;
        private ToolStripProgressBar progress;
        private WebView2 web;
        private readonly AutoCompleteStringCollection _history = new AutoCompleteStringCollection();
        private double _zoomFactor = 1.0;

        public MainForm()
        {
            Text = "Mi Navegador";
            Width = 1100; Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            Icon = SystemIcons.Application;
            InitializeUi();
            InitializeWebView2Async();
        }

        private void InitializeUi()
        {
            ts = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, ImageScalingSize = new Size(18, 18) };
            btnBack    = new ToolStripButton("⟵") { ToolTipText = "Atrás (Alt+Izq)", Enabled = false };
            btnForward = new ToolStripButton("⟶") { ToolTipText = "Adelante (Alt+Der)", Enabled = false };
            btnRefresh = new ToolStripButton("⟳") { ToolTipText = "Recargar (F5)" };
            btnStop    = new ToolStripButton("■") { ToolTipText = "Detener" };
            btnHome    = new ToolStripButton("⌂") { ToolTipText = "Inicio (google.com)" };
            lblLock = new ToolStripLabel(" ") { ToolTipText = "Estado de seguridad" };

            txtAddress = new ToolStripTextBox { AutoSize = false, Width = 520, BorderStyle = BorderStyle.FixedSingle };
            txtAddress.ToolTipText = "Escribe una URL o búsqueda y presiona Enter";
            txtAddress.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtAddress.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtAddress.AutoCompleteCustomSource = _history;

            cboSearch = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            cboSearch.Items.AddRange(new[] { "Google", "Bing", "DuckDuckGo" });
            cboSearch.SelectedIndex = 0;

            btnFavAdd = new ToolStripButton("★+") { ToolTipText = "Añadir a Favoritos" };
            ddFavs = new ToolStripDropDownButton("Favoritos");
            btnFind = new ToolStripButton("🔎") { ToolTipText = "Buscar en la página (Ctrl+F)" };
            txtFind = new ToolStripTextBox() { Visible = false, Width = 180, ToolTipText = "Buscar en la página… (Enter: siguiente, Shift+Enter: anterior, Esc: cerrar)" };

            btnZoomOut = new ToolStripButton("−") { ToolTipText = "Zoom − (Ctrl+−)" };
            btnZoomReset = new ToolStripButton("100%") { ToolTipText = "Restablecer zoom (Ctrl+0)" };
            btnZoomIn = new ToolStripButton("+") { ToolTipText = "Zoom + (Ctrl+=)" };

            ddMenu = new ToolStripDropDownButton("⋮");
            ddMenu.DropDownItems.Add("Nueva ventana (Ctrl+N)", null, (s, e) => NewWindow());
            ddMenu.DropDownItems.Add(new ToolStripSeparator());
            ddMenu.DropDownItems.Add("Ver código fuente (Ctrl+U)", null, (s, e) => ViewSource());
            ddMenu.DropDownItems.Add("Copiar URL actual", null, (s, e) => { if (!string.IsNullOrEmpty(web?.Source?.ToString())) Clipboard.SetText(web.Source.ToString()); });
            ddMenu.DropDownItems.Add(new ToolStripSeparator());
            ddMenu.DropDownItems.Add("Salir (Alt+F4)", null, (s, e) => Close());

            ts.Items.AddRange(new ToolStripItem[]
            {
                btnBack, btnForward, new ToolStripSeparator(),
                btnRefresh, btnStop, btnHome, new ToolStripSeparator(),
                lblLock, txtAddress, new ToolStripSeparator(),
                cboSearch, btnFavAdd, ddFavs, new ToolStripSeparator(),
                btnFind, txtFind, new ToolStripSeparator(),
                btnZoomOut, btnZoomReset, btnZoomIn, new ToolStripSeparator(),
                ddMenu
            });

            status = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Listo");
            progress = new ToolStripProgressBar { Minimum = 0, Maximum = 100, Size = new Size(120, 16), Visible = false };
            lblProgress = new ToolStripStatusLabel("");
            status.Items.AddRange(new ToolStripItem[] { lblStatus, progress, lblProgress });

            web = new WebView2 { Dock = DockStyle.Fill };

            Controls.AddRange(new Control[] { web, ts, status });

            btnBack.AccessibleName = "Atrás"; btnForward.AccessibleName = "Adelante";
            btnRefresh.AccessibleName = "Recargar"; btnStop.AccessibleName = "Detener"; btnHome.AccessibleName = "Inicio";
            txtAddress.AccessibleName = "Barra de direcciones"; txtFind.AccessibleName = "Buscar en la página";
            ddFavs.AccessibleName = "Favoritos"; btnFavAdd.AccessibleName = "Añadir a Favoritos";

            btnBack.Click += (s, e) => { if (web.CoreWebView2 != null && web.CoreWebView2.CanGoBack) web.CoreWebView2.GoBack(); };
            btnForward.Click += (s,e) => { if (web.CoreWebView2 != null && web.CoreWebView2.CanGoForward) web.CoreWebView2.GoForward(); };
            btnRefresh.Click += (s, e) => web.Reload();
            btnStop.Click += (s, e) => web.Stop();
            btnHome.Click += (s, e) => NavigateTo("https://www.google.com/");
            btnFavAdd.Click += (s, e) => AddFavorite();
            btnFind.Click += (s, e) => ToggleFind();
            btnZoomOut.Click += (s, e) => SetZoom(_zoomFactor - 0.1);
            btnZoomReset.Click += (s, e) => SetZoom(1.0);
            btnZoomIn.Click += (s, e) => SetZoom(_zoomFactor + 0.1);

            txtAddress.KeyDown += TxtAddress_KeyDown;
            txtFind.KeyDown += TxtFind_KeyDown;

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
        }

        private async void InitializeWebView2Async()
        {
            lblStatus.Text = "Inicializando motor...";
            try
            {
                await web.EnsureCoreWebView2Async();
                web.CoreWebView2InitializationCompleted += Web_CoreWebView2InitializationCompleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible inicializar WebView2.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Web_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show("Error al inicializar WebView2: " + e.InitializationException?.Message);
                return;
            }
            web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            web.CoreWebView2.Settings.IsStatusBarEnabled = false;
            web.CoreWebView2.Settings.IsZoomControlEnabled = false;
            web.CoreWebView2.Settings.AreDevToolsEnabled = true;

            web.NavigationStarting += Web_NavigationStarting;
            web.NavigationCompleted += Web_NavigationCompleted;
            web.SourceChanged += Web_SourceChanged;
            web.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
            web.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            web.CoreWebView2.FrameNavigationStarting += (s, ev) => ShowProgress(true);
            web.CoreWebView2.FrameNavigationCompleted += (s, ev) => ShowProgress(false);
            web.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

            NavigateTo("https://www.google.com/");
        }

        private void TxtAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                NavigateFromAddressBar();
            }
        }

        private void NavigateFromAddressBar()
        {
            var input = txtAddress.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            if (!input.Contains(".") && !input.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                input = BuildSearchQuery(input);
            }
            else if (!input.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                input = "https://" + input;
            }
            NavigateTo(input);
        }

        private string BuildSearchQuery(string q)
        {
            string enc = Uri.EscapeDataString(q);
            switch (cboSearch.SelectedItem?.ToString())
            {
                case "Bing":       return $"https://www.bing.com/search?q={enc}";
                case "DuckDuckGo": return $"https://duckduckgo.com/?q={enc}";
                default:           return $"https://www.google.com/search?q={enc}";
            }
        }

        private void NavigateTo(string url)
        {
            try
            {
                web.Source = new Uri(url);
                if (!_history.Contains(url)) _history.Add(url);
                lblStatus.Text = "Cargando " + url;
                ShowProgress(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("URL inválida.\n" + ex.Message, "Navegación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Web_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            lblLock.Text = e.Uri.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "🔒" : "⚠";
            lblLock.ToolTipText = e.Uri.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "Conexión segura (HTTPS)" : "Sitio no seguro";
            lblStatus.Text = "Navegando…";
            ShowProgress(true);
        }

        private void Web_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            ShowProgress(false);
            if (e.IsSuccess)
            {
                lblStatus.Text = "Completado";
                txtAddress.Text = web.Source?.ToString() ?? "";
            }
            else
            {
                lblStatus.Text = "Error de navegación";
                MessageBox.Show($"No se pudo cargar la página.\nCódigo: {e.WebErrorStatus}", "Navegación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Web_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txtAddress.Text = web.Source?.ToString() ?? "";
        }

        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            btnBack.Enabled = web.CoreWebView2.CanGoBack;
            btnForward.Enabled = web.CoreWebView2.CanGoForward;
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            Text = string.IsNullOrEmpty(web.CoreWebView2.DocumentTitle) ? "Mi Navegador" : $"Mi Navegador — {web.CoreWebView2.DocumentTitle}";
        }

        private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var defPath = e.ResultFilePath;
            using (var sfd = new SaveFileDialog { FileName = System.IO.Path.GetFileName(defPath) })
            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    e.ResultFilePath = sfd.FileName;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            var download = e.DownloadOperation;
            download.BytesReceivedChanged += (s, ev) =>
            {
                Invoke((Action)(() =>
                {
                    progress.Visible = true;
                    if (download.TotalBytesToReceive > 0)
                    {
                        int p = (download.TotalBytesToReceive > 0 ? (int)((download.BytesReceived * 100.0) / (double)download.TotalBytesToReceive) : 0);
                        progress.Value = Math.Max(0, Math.Min(100, p));
                        lblProgress.Text = $"{p}%";
                    }
                }));
            };
            download.StateChanged += (s, ev) =>
            {
                Invoke((Action)(() =>
                {
                    if (download.State == CoreWebView2DownloadState.Completed)
                    {
                        lblStatus.Text = "Descarga completada";
                        progress.Visible = false; lblProgress.Text = "";
                    }
                    else if (download.State == CoreWebView2DownloadState.Interrupted)
                    {
                        lblStatus.Text = "Descarga interrumpida";
                        progress.Visible = false; lblProgress.Text = "";
                    }
                }));
            };
        }

        private void ShowProgress(bool isLoading)
        {
            progress.Visible = isLoading;
            if (isLoading) { progress.Style = ProgressBarStyle.Marquee; }
            else { progress.Style = ProgressBarStyle.Blocks; progress.Value = 0; lblProgress.Text = ""; }
        }

        private void AddFavorite()
        {
            var url = web?.Source?.ToString();
            if (string.IsNullOrWhiteSpace(url)) return;
            foreach (ToolStripItem it in ddFavs.DropDownItems)
                if (it.Text == url) { MessageBox.Show("Ya está en Favoritos."); return; }

            ddFavs.DropDownItems.Add(url, null, (s, e) => NavigateTo(url));
        }

        private void ToggleFind()
        {
            txtFind.Visible = !txtFind.Visible;
            if (txtFind.Visible) { txtFind.Focus(); txtFind.SelectAll(); }
        }

        private static string EscapeForJs(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private void TxtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { ToggleFind(); e.SuppressKeyPress = true; return; }
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                bool forward = (ModifierKeys & Keys.Shift) == 0; // Shift+Enter = buscar atrás
                string text = EscapeForJs(txtFind.Text);
                string js = $"window.find(\"{text}\", false, {(forward ? "false" : "true")}, true, false, true, false);";
                _ = web.CoreWebView2?.ExecuteScriptAsync(js);
            }
        }

        private void SetZoom(double factor)
        {
            _zoomFactor = Math.Max(0.5, Math.Min(3.0, factor));
            // CSS zoom para evitar problemas de API ZoomFactor (compatible con todas las versiones)
            if (web?.CoreWebView2 != null)
            {
                string percent = ((int)(_zoomFactor * 100.0 + 0.5)).ToString();
                string js = $"document.body.style.zoom='{percent}%';";
                _ = web.CoreWebView2.ExecuteScriptAsync(js);
            }
            btnZoomReset.Text = ((int)(_zoomFactor * 100.0 + 0.5)).ToString() + "%";
        }

        private void NewWindow()
        {
            var win = new MainForm();
            win.Show();
        }

        private async void ViewSource()
        {
            try
            {
                string html = await web.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                html = System.Text.RegularExpressions.Regex.Unescape(html.Trim('\"'));
                var frm = new Form { Text = "Código fuente", Width = 900, Height = 700, StartPosition = FormStartPosition.CenterParent };
                var txt = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9), ReadOnly = true, Text = html, WordWrap = false };
                frm.Controls.Add(txt);
                frm.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo obtener el código fuente.\n" + ex.Message);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L) { txtAddress.Focus(); txtAddress.SelectAll(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.F) { ToggleFind(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.F5) { web.Reload(); e.SuppressKeyPress = true; }
            else if (e.Alt && e.KeyCode == Keys.Left) { if (web.CoreWebView2.CanGoBack) web.CoreWebView2.GoBack(); e.SuppressKeyPress = true; }
            else if (e.Alt && e.KeyCode == Keys.Right){ if (web.CoreWebView2.CanGoForward) web.CoreWebView2.GoForward(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.Oemplus) { SetZoom(_zoomFactor + 0.1); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.OemMinus) { SetZoom(_zoomFactor - 0.1); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D0) { SetZoom(1.0); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.N) { NewWindow(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.U) { ViewSource(); e.SuppressKeyPress = true; }
        }
    }
}
