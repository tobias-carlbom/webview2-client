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
using System.Security.Cryptography;

namespace Webview2Client
{
    public partial class MainForm : Form
    {
        private string currentUrl;
        private string adminPassword;
        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "Webview2Client";

        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += MainForm_FormClosing;
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            currentUrl = ConfigurationManager.AppSettings["URL"] ?? "http://localhost:1881";

            string encryptedPassword = ConfigurationManager.AppSettings["AdminPassword"] ?? "";
            adminPassword = string.IsNullOrEmpty(encryptedPassword) ? "" : DecryptString(encryptedPassword);

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

        private void SaveConfiguration(string url, bool startMaximized, bool hideToolbar, bool hideTitleBar, bool autostart, string adminPassword)
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

                string encryptedPassword = string.IsNullOrEmpty(adminPassword) ? "" : EncryptString(adminPassword);
                if (config.AppSettings.Settings["AdminPassword"] == null)
                    config.AppSettings.Settings.Add("AdminPassword", encryptedPassword);
                else
                    config.AppSettings.Settings["AdminPassword"].Value = encryptedPassword;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save configuration: {ex.Message}", "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

        private bool ValidateAdminAccess()
        {
            if (string.IsNullOrWhiteSpace(adminPassword))
                return true;

            using (var passwordForm = new PasswordForm())
            {
                if (passwordForm.ShowDialog() == DialogResult.OK)
                {
                    return passwordForm.Password == adminPassword;
                }
            }
            return false;
        }

        public void EditApplicationSettings()
        {
            if (!ValidateAdminAccess())
            {
                MessageBox.Show("Access denied. Incorrect password.", "Authentication Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SettingsForm configForm = new SettingsForm())
            {
                configForm.WebsiteUrl = currentUrl;
                configForm.StartMaximized = bool.Parse(ConfigurationManager.AppSettings["StartMaximized"] ?? "false");
                configForm.HideToolbar = bool.Parse(ConfigurationManager.AppSettings["HideToolbar"] ?? "false");
                configForm.HideTitleBar = bool.Parse(ConfigurationManager.AppSettings["HideTitleBar"] ?? "false");
                configForm.Autostart = bool.Parse(ConfigurationManager.AppSettings["Autostart"] ?? "false");
                configForm.AdminPassword = adminPassword;

                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    string newUrl = configForm.WebsiteUrl;
                    bool startMaximized = configForm.StartMaximized;
                    bool hideToolbar = configForm.HideToolbar;
                    bool hideTitleBar = configForm.HideTitleBar;
                    bool autostart = configForm.Autostart;
                    string newAdminPassword = configForm.AdminPassword;

                    if (!string.IsNullOrWhiteSpace(newUrl) && newUrl != currentUrl)
                    {
                        currentUrl = newUrl;
                        webView21.CoreWebView2.Navigate(newUrl);
                    }

                    adminPassword = newAdminPassword;
                    SaveConfiguration(newUrl, startMaximized, hideToolbar, hideTitleBar, autostart, newAdminPassword);
                    ApplyRuntimeSettings(hideToolbar, hideTitleBar);
                    SetAutostart(autostart);
                }
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

        private void Form1_Load(object sender, EventArgs e)
        {
            InitBrowser();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                if (!ValidateAdminAccess())
                {
                    e.Cancel = true;
                    MessageBox.Show("Access denied. Incorrect password.", "Authentication Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void editUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditApplicationSettings();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            EditApplicationSettings();
        }

        private string EncryptString(string text)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(text);
                byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(data, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedData);
            }
            catch
            {
                return text;
            }
        }

        private string DecryptString(string encryptedText)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] data = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return encryptedText;
            }
        }
    }
}