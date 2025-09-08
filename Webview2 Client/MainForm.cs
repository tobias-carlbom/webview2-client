using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Configuration;
using Microsoft.Win32;

namespace Webview2Client
{
    public partial class MainForm : Form
    {
        private string currentUrl;
        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "Webview2Client";

        public MainForm()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            currentUrl = ConfigurationManager.AppSettings["URL"] ?? "http://localhost:1881";

            bool startMaximized = bool.Parse(ConfigurationManager.AppSettings["StartMaximized"] ?? "false");
            if (startMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            bool hideToolbar = bool.Parse(ConfigurationManager.AppSettings["HideToolbar"] ?? "false");
            if (hideToolbar)
            {
                if (this.Controls.Find("menuStrip1", true).Length > 0)
                {
                    ((MenuStrip)this.Controls.Find("menuStrip1", true)[0]).Visible = false;
                }
            }

            bool hideTitleBar = bool.Parse(ConfigurationManager.AppSettings["HideTitleBar"] ?? "false");
            if (hideTitleBar)
            {
                this.FormBorderStyle = FormBorderStyle.None;
            }

            bool autostart = bool.Parse(ConfigurationManager.AppSettings["Autostart"] ?? "false");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitBrowser();
        }

        private async Task Initialized()
        {
            await webView21.EnsureCoreWebView2Async(null);
        }

        public async void InitBrowser()
        {
            await Initialized();
            webView21.CoreWebView2.Navigate(currentUrl);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void editUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditApplicationSettings();
        }

        public void EditApplicationSettings()
        {
            using (ApplicationSettings configForm = new ApplicationSettings())
            {
                configForm.WebsiteUrl = currentUrl;
                configForm.StartMaximized = bool.Parse(ConfigurationManager.AppSettings["StartMaximized"] ?? "false");
                configForm.HideToolbar = bool.Parse(ConfigurationManager.AppSettings["HideToolbar"] ?? "false");
                configForm.HideTitleBar = bool.Parse(ConfigurationManager.AppSettings["HideTitleBar"] ?? "false");
                configForm.Autostart = bool.Parse(ConfigurationManager.AppSettings["Autostart"] ?? "false");

                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    string newUrl = configForm.WebsiteUrl;
                    bool startMaximized = configForm.StartMaximized;
                    bool hideToolbar = configForm.HideToolbar;
                    bool hideTitleBar = configForm.HideTitleBar;
                    bool autostart = configForm.Autostart;

                    if (!string.IsNullOrWhiteSpace(newUrl) && newUrl != currentUrl)
                    {
                        currentUrl = newUrl;
                        webView21.CoreWebView2.Navigate(newUrl);
                    }

                    SaveConfiguration(newUrl, startMaximized, hideToolbar, hideTitleBar, autostart);
                    ApplyRuntimeSettings(hideToolbar, hideTitleBar);
                    SetAutostart(autostart);
                }
            }
        }

        private void SaveConfiguration(string url, bool startMaximized, bool hideToolbar, bool hideTitleBar, bool autostart)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings["URL"] == null)
                    config.AppSettings.Settings.Add("URL", url);
                else
                    config.AppSettings.Settings["URL"].Value = url;

                if (config.AppSettings.Settings["StartMaximized"] == null)
                    config.AppSettings.Settings.Add("StartMaximized", startMaximized.ToString());
                else
                    config.AppSettings.Settings["StartMaximized"].Value = startMaximized.ToString();

                if (config.AppSettings.Settings["HideToolbar"] == null)
                    config.AppSettings.Settings.Add("HideToolbar", hideToolbar.ToString());
                else
                    config.AppSettings.Settings["HideToolbar"].Value = hideToolbar.ToString();

                if (config.AppSettings.Settings["HideTitleBar"] == null)
                    config.AppSettings.Settings.Add("HideTitleBar", hideTitleBar.ToString());
                else
                    config.AppSettings.Settings["HideTitleBar"].Value = hideTitleBar.ToString();

                if (config.AppSettings.Settings["Autostart"] == null)
                    config.AppSettings.Settings.Add("Autostart", autostart.ToString());
                else
                    config.AppSettings.Settings["Autostart"].Value = autostart.ToString();

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save configuration: {ex.Message}", "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetAutostart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    if (enable)
                    {
                        string executablePath = Application.ExecutablePath;
                        key?.SetValue(APP_NAME, executablePath);
                    }
                    else
                    {
                        key?.DeleteValue(APP_NAME, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not update autostart setting: {ex.Message}", "Autostart Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsAutostartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    return key?.GetValue(APP_NAME) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ApplyRuntimeSettings(bool hideToolbar, bool hideTitleBar)
        {
            if (this.Controls.Find("menuStrip1", true).Length > 0)
            {
                ((MenuStrip)this.Controls.Find("menuStrip1", true)[0]).Visible = !hideToolbar;
            }

            bool currentlyHideTitleBar = (this.FormBorderStyle == FormBorderStyle.None);
            if (hideTitleBar != currentlyHideTitleBar)
            {
                MessageBox.Show("Title bar changes will take effect after restarting the application.",
                    "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            EditApplicationSettings();
        }
    }
}