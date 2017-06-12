using MigrationSample.Core;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    public class ReportsGridSourceManager
    {
        public BindingSource BindingSource { get; private set; }

        public object DataSource { set { BindingSource.DataSource = value; } }

        public List<ReportMigrationData> FilteredReports { get; set; }

        public ReportsGridSourceManager(DataGridView dataGridView)
        {
            BindingSource = new BindingSource();
            dataGridView.DataSource = BindingSource;
        }

        public virtual void UpdateSource()
        {
            BindingSource.ResetBindings(false);
        }
    }
}
