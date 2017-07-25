using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    public class ExportSourceManager : ReportsGridSourceManager
    {
        public ExportSourceManager(DataGridView dataGridView) : base(dataGridView)
        {

        }

        public override void UpdateSource()
        {
            DataSource = FilteredReports.Select(r => new
            {
                PaaSReportName = r.PaaSReportName,
                PBIXExists = File.Exists(r.PbixPath) ? "Yes" : "No",
                ExportState = r.ExportState,
                LastExportStatus = r.LastExportStatus,
                PaaSWorkspaceCollectionName = r.PaaSWorkspaceCollectionName,
                PaaSWorkspaceId = r.PaaSWorkspaceId,
                PaaSReportId = r.PaaSReportId,
                PbixPath = r.PbixPath,
            });

            base.UpdateSource();
        }
    }
}
