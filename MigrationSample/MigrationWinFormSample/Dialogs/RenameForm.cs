using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigrationSample.Dialogs
{
    public partial class RenameForm : Form
    {
        public string Result
        {
            get
            {
                return this.txtResult.Text;
            }
        }

        public RenameForm(string prevName)
        {
            InitializeComponent();
            this.txtResult.Text = prevName;
        }
        

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
