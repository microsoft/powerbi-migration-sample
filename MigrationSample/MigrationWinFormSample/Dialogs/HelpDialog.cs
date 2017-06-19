using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigrationWinFormSample.Dialogs
{

    public partial class HelpDialog : Form
    {
        private string HelpUrl { get; set; }

        public HelpDialog(string title, string text, string helpUrl)
        {
            InitializeComponent();

            InstractionsLbl.Text = text;

            this.Text = title;

            HelpUrl = helpUrl;
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            Process.Start(HelpUrl);
        }

        private void OKbtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
