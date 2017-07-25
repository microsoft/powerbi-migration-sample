using System.Linq;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    class ImportSourceManager : ReportsGridSourceManager
    {
        public ImportSourceManager(DataGridView dataGridView) : base(dataGridView)
        {
        }

        public override void UpdateSource()
        {
            DataSource = FilteredReports.Select(r => new
            {
                TargetName = r.SaaSTargetReportName,
                UploadState = r.SaaSImportState,
                UploadError = r.SaaSImportError,
                TargetGroupName = r.SaaSTargetGroupName,
                TargetGroupCreatedId = r.SaaSTargetGroupId,
                UploadedReportId = r.SaaSReportId,
            });

            base.UpdateSource();
        }
    }
}
