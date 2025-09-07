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

        private void button1_Click(object sender, EventArgs e)
        {
            // Optional: Add validation here
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid URL.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Don't close the dialog
            }

            // If validation passes, the DialogResult.OK will automatically close the dialog
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // No validation needed for cancel
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
