using MigrationSample.Core;
using System.Linq;
using System.Windows.Forms;

namespace MigrationSample.Auxilary
{
    class AnalyzeSourceManager : ReportsGridSourceManager
    {
        public static class ReportTypeStrings
        {
            public static string PushDataset = "Pushed Data Set.";
            public static string ImportedDirectQuery = "Imported Dataset. Direct Query.";
            public static string ImportedCached = "Imported Dataset. Cached Data.";
            public static string OldImport = $"Imported Dataset. Created before {ReportMigrationData.MinimalSupportedImportUpdateDate.ToString("MM/dd/yyyy")}.";
        }

        public static class DownloadabilityStrings
        {
            public static string CanBeDownloaded = "Yes. Can be downloaded.";
            public static string ShouldBeCreatedFromLocal = "No. Should be created from local pbix.";
            public static string ShouldBeCreatedFromJson = "No. Should be recreated from json.";
        }

        public AnalyzeSourceManager(DataGridView dataGridView) : base(dataGridView)
        {

        }

        public override void UpdateSource()
        {
            DataSource = FilteredReports.Select(r => new
            {
                PaaSReportName = r.PaaSReportName,
                Type = (r.IsBoundToOldDataset) ? ReportTypeStrings.OldImport :
                    (r.IsPushDataset) ? ReportTypeStrings.PushDataset :
                    (!string.IsNullOrWhiteSpace(r.DirectQueryConnectionString)) ? ReportTypeStrings.ImportedDirectQuery :
                    ReportTypeStrings.ImportedCached,
                Downloadable = (r.IsBoundToOldDataset) ? DownloadabilityStrings.ShouldBeCreatedFromLocal :
                    (r.IsPushDataset) ? DownloadabilityStrings.ShouldBeCreatedFromJson : DownloadabilityStrings.CanBeDownloaded,
            });

            base.UpdateSource();
        }
    }
}
