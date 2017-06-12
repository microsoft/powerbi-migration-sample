using Microsoft.PowerBI.Api.V1.Models;
using System;

namespace MigrationSample.Core
{
    public enum ExportState
    {
        Failed,
        InProgress,
        Done
    }

    public enum ImportState
    {
        Failed,
        InProgress,
        Done,
        Publishing
    }

    public class ReportMigrationData
    {
        public static readonly DateTime MinimalSupportedImportUpdateDate = new DateTime(2016, 11, 26);

        public string PaaSWorkspaceCollectionName { get; set; }

        public string PaaSWorkspaceId { get; set; }

        public string PaaSReportId { get; set; }

        public string PaaSReportLastImportTime { get; set; }

        public string PaaSReportName { get; set; }

        public bool IsPushDataset { get; set; }

        public bool IsBoundToOldDataset { get; set; }

        public string PbixPath { get; set; }

        public ExportState? ExportState { get; set; }

        public string LastExportStatus { get; set; }

        public string SaaSTargetGroupName { get; set; }

        public string SaaSTargetGroupId { get; set; }

        public string SaaSTargetGroupCreationStatus { get; set; }

        public string SaaSTargetReportName { get; set; }

        public ImportState? SaaSImportState { get; set; }

        public string SaaSReportId { get; set; }

        public string SaaSImportError { get; set; }

        public string DirectQueryConnectionString { get; set; }

        /// <summary>
        /// This constructor is required for serialization
        /// </summary>
        public ReportMigrationData()
        {
        }

        public void ResetImportProgressData()
        {
            SaaSTargetGroupCreationStatus = null;
            SaaSTargetGroupId = null;
            SaaSImportError = null;
            SaaSImportState = null;
            SaaSReportId = null;
        }

        public ReportMigrationData(Report report, Import import, Dataset dataset, ODataResponseListDatasource datasources, string workspaceCollectionName, string workspaceId)
        {
            IsPushDataset = (dataset.AddRowsAPIEnabled.HasValue) ? dataset.AddRowsAPIEnabled.Value : false;

            if (import == null)
            {
                IsBoundToOldDataset = false;
            }
            else
            {
                PaaSReportLastImportTime = import.UpdatedDateTime.ToString();

                IsBoundToOldDataset = import.UpdatedDateTime < MinimalSupportedImportUpdateDate;

                if (datasources != null)
                {
                    DirectQueryConnectionString = datasources.Value[0].ConnectionString;
                }
            }

            PaaSReportId = report.Id;
            PaaSReportName = report.Name;
            PaaSWorkspaceCollectionName = workspaceCollectionName;
            PaaSWorkspaceId = workspaceId;
        }
    }
}
