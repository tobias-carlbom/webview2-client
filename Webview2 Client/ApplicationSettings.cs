using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webview2Client
{
    public partial class ApplicationSettings : Form
    {
        public ApplicationSettings()
        {
            InitializeComponent();
        }

        // Public properties to access form values
        public string WebsiteUrl
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public bool StartMaximized
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }

        public bool HideToolbar
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }

        public bool HideTitleBar
        {
            get { return checkBox3.Checked; }
            set { checkBox3.Checked = value; }
        }

        public bool Autostart
        {
            get { return checkBox4.Checked; }
            set { checkBox4.Checked = value; }
        }

        public string AdminPassword
        {
            get { return textBoxPassword.Text; }
            set { textBoxPassword.Text = value; }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid URL.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
