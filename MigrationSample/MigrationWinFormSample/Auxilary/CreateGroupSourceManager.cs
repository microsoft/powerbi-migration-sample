using System.Linq;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    class CreateGroupSourceManager : ReportsGridSourceManager
    {
        public CreateGroupSourceManager(DataGridView dataGridView) : base(dataGridView)
        {

        }

        public override void UpdateSource()
        {
            DataSource = FilteredReports.Select(r => new
            {
                GroupName = r.SaaSTargetGroupName,
                GroupCreationStatus = r.SaaSTargetGroupCreationStatus,
                ForReport = r.SaaSTargetReportName,
                CreatedGroupId = r.SaaSTargetGroupId,
            });

            base.UpdateSource();
        }
    }
}
