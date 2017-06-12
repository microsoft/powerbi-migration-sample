using System.Collections.Generic;

namespace MigrationSample.Core
{
    public class AnalyzedWorkspaceCollection
    {
        public string  WorkspaceCollectionName { get; set; }

        public List<AnalyzedWorkspace> Workspaces { get; set; }

        public int ExportableReportsCnt { get; set; }

        public int AllReportsCnt { get; set; }

        public AnalyzedWorkspaceCollection(string wcName)
        {
            WorkspaceCollectionName = wcName;

            Workspaces = new List<AnalyzedWorkspace>();
        }
    }

    public class AnalyzedWorkspace
    {
        public string WorkspaceId { get; set; }

        public List<ReportMigrationData> Reports { get; set; }

        public int ExportableReportsCnt { get; set; }

        public AnalyzedWorkspace(string workspaceId)
        {
            WorkspaceId = workspaceId;

            Reports = new List<ReportMigrationData>();
        }
    }
}


