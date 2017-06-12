using System.Linq;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    class UpdateCredentialsSourceManager : ReportsGridSourceManager
    {
        public UpdateCredentialsSourceManager(DataGridView dataGridView) : base(dataGridView)
        {

        }

        public override void UpdateSource()
        {
            DataSource = FilteredReports.Select(r => new
            {
                TargetGroupName = r.SaaSTargetGroupName,
                TargetReportName = r.SaaSTargetReportName,
                DirectQueryConnectionString = r.DirectQueryConnectionString,
            });

            base.UpdateSource();
        }
    }
}
