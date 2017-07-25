using MigrationSample.Auxilary;
using MigrationSample.Core;
using MigrationSample.Dialogs;
using MigrationSample.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace MigrationSample
{
    public partial class MigrationTabForm : ProvisioningFormPage
    {
        private class MigrationPlanActions
        {
            public const string DeleteWorkspace = "Delete Workspace from Migration Plan";
            public const string DeleteWorkspaceCollection = "Delete Workspace Collection from Migration Plan";
            public const string RenameGroup = "Rename Group";
        }

        private class Errors
        {
            public class Import
            {
                public const string DuplicateReport = "Duplicate report";
                public const string GroupIdsMismatch = "GroupIDs don't match";
                public const string GroupWasNotFound = "The target group was not found";
                public const string UploadFailed = "Upload Failed";
            }

            public class Export
            {
                public const string PBIXNotCreated = "PBIX file was not created";
                public const string DownloadFailed = "Download failed";
            }

            public class GroupCreation
            {
                public const string FailedToCreateGroup = "Failed to Create Group";
            }
        }

        public MigrationPlan MigrationPlan { get; private set; }

        public string DisplayName
        {
            get
            {
                return MigrationPlan.Context.DisplayName;
            }
        }

        private int ExportableReportsCnt { get; set; }

        private int AllReportsCnt { get; set; }

        private List<AnalyzedWorkspaceCollection> AnalyzedWorkspaceCollections { get; set; }

        private ReportsGridSourceManager AnalyzedReportsSourceManager { get; set; }

        private ReportsGridSourceManager ExportSourceManager { get; set; }

        private ReportsGridSourceManager GroupsSourceManager { get; set; }

        private ReportsGridSourceManager ImportSourceManager { get; set; }

        private ReportsGridSourceManager ConnectionsSourceManager { get; set; }

        private string MigrationPlanFile { get; set; }

        /// <summary>
        /// this constructor is used when the tab is create from existing file
        /// </summary>
        /// <param name="migrationPlanFilePath"></param>
        public MigrationTabForm(MigrationPlan migrationPlan, string migrationPlanFilePath)
        {
            Init();

            MigrationPlanFile = migrationPlanFilePath;

            MigrationPlan = migrationPlan;

            AnalyzedReportsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(r => !string.IsNullOrWhiteSpace(r.PaaSReportId)).ToList();

            SetRootDirectory();

            if (MigrationPlan.ReportsMigrationData != null)
            {
                GenerateAnalyzedWorkspaceCollections();
                UpdateActualPaaSTopologyTreeAndStatistics();
                UpdateAnalyzeGrid();
                UpdateImportToSaaSTreeView();
                UpdateExportTreeView();
            }
        }

        /// <summary>
        /// this constructor is used when the new tab is created
        /// </summary>
        /// <param name="context"></param>
        public MigrationTabForm(PBIProvisioningContext context)
        {
            Init();

            MigrationPlan = new MigrationPlan(context);

            SetRootDirectory();
        }

        public async Task BlockTabAndExecuteAsync(Func<Task> func)
        {
            this.flowLayoutPanel.Enabled = false;
            try
            {
                await func();
            }
            catch (AdalException e)
            {
                MessageBox.Show($"Reason: {e.ErrorCode}", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            finally
            {
                this.flowLayoutPanel.Enabled = true;
            }
        }

        public async Task BlockTabAndRunTask(Func<Task> func)
        {
            await BlockTabAndExecuteAsync(async () => await Task.Run(() => func()));
        }

        public void UpdateAnalyzeGrid()
        {
            if (AnalyzedReportsSourceManager.FilteredReports == null)
            {
                return;
            }

            AnalyzedReportsSourceManager.UpdateSource();

            if (analyzeGridView.Columns["Type"] != null && analyzeGridView.Columns["Type"].Visible)
            {
                analyzeGridView.Columns["Type"].Visible = false;
                DataGridViewLinkColumn col = new DataGridViewLinkColumn();
                col.DataPropertyName = "Type";
                col.Name = "Documentation";
                analyzeGridView.Columns.Insert(3, col);
            }

            analyzeGridView.Columns[0].HeaderText = "PaaS Report Name";
        }

        public string GetMigrationPlanFile()
        {
            SaveMigrationPlan();

            return MigrationPlanFile;
        }

        public void SaveMigrationPlanAs(string path = null)
        {
            var prev = MigrationPlanFile;

            MigrationPlanFile = null;
            try
            {
                SaveMigrationPlan();
            }
            finally
            {
                if (!File.Exists(MigrationPlanFile))
                {
                    MigrationPlanFile = prev;
                }
            }
        }

        public void SaveMigrationPlan(string path = null)
        {
            if (path == null)
            {
                if (!string.IsNullOrWhiteSpace(MigrationPlanFile))
                {
                    path = MigrationPlanFile;
                }
                else
                {
                    path = MainForm.SelectFileToSave(rootFolderTb.Text, $"{MigrationPlan.Context.Environment}-{MigrationPlan.Context.Subscription.DisplayName}-{MigrationPlan.Context.ResourceGroupName}.xml");

                    if (path == null)
                    {
                        return;
                    }
                }
            }

            try
            {
                MigrationPlan.Save(path);
            }
            catch
            {
                MessageBox.Show($"Failed to save MigrationPlan to {path}");
                return;
            }

            MigrationPlanFile = path;
        }

        private void SetRootDirectory()
        {
            if (MigrationPlan.Context.ResourceGroupName != null)
            {
                if (string.IsNullOrWhiteSpace(MigrationPlan.MigrationRootPath))
                {
                    MigrationPlan.MigrationRootPath = $"{ConfigurationManager.AppSettings["MigrationRoot"]}\\{MigrationPlan.Context.ResourceGroupName}";
                }

                rootFolderTb.Text = MigrationPlan.MigrationRootPath;
            }
        }

        private void Init()
        {
            InitializeComponent();

            this.flowLayoutPanel = mainMigrationTlp;

            AnalyzedReportsSourceManager = new AnalyzeSourceManager(analyzeGridView);
            ExportSourceManager = new ExportSourceManager(exportGridView);
            GroupsSourceManager = new CreateGroupSourceManager(groupsGridView);
            ImportSourceManager = new ImportSourceManager(importGridView);

            nameConflictCB.Items.Add("Abort");
            nameConflictCB.Items.Add("Ignore");
            nameConflictCB.Items.Add("Overwrite");
            nameConflictCB.SelectedIndex = 0;

            ResizeItemSize();
        }

        public void ResizeItemSize()
        {
            mainMigrationTc.ItemSize = new Size(MainForm.MainControl.Width / mainMigrationTc.TabCount -10, 0);
        }

        private async Task AnalyzePaaSForMigration()
        {
            // If this is not the first time the MigrationPlan is created
            // Warn user that the data will be lost
            if (MigrationPlan.ReportsMigrationData.Any())
            {
                var dialogResult = MessageBox.Show(
                    "This resource group has already been analyzed. Do you want to continue and overwrite existing data? ",
                    "Migration Sample", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }

                MigrationPlan.ReportsMigrationData.Clear();
            }

            await BlockTabAndRunTask(async () => await AnalyzeResourceGroup());
        }

        private async Task AnalyzeResourceGroup()
        {
            try
            {
                var paaSController = new PaaSController(MigrationPlan.Context);
                var collections = await paaSController.GetRelevantWorkspaceCollections();

                this.AnalyzedWorkspaceCollections = collections.Select(wc => new AnalyzedWorkspaceCollection(wc.Name)).ToList();

                await Task.WhenAll(this.AnalyzedWorkspaceCollections.Select(awc => AnalyzeWorkspaceCollection(awc)).ToArray());

                foreach (var analyzedWSCollection in AnalyzedWorkspaceCollections)
                {
                    foreach (var analyzedWS in analyzedWSCollection.Workspaces)
                    {
                        foreach (var reportMigrationData in analyzedWS.Reports)
                        {
                            MigrationPlan.ReportsMigrationData.Add(reportMigrationData);
                        }
                    }
                }

                RunInUIContext(() =>
                {
                    AnalyzedReportsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData;

                    UpdateActualPaaSTopologyTreeAndStatistics();
                    UpdateAnalyzeGrid();
                    UsePaaSTopologyForExport();
                    UsePaaSTopologyForImport();

                    SaveMigrationPlan();
                });
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to analyze Resource Group. Error: { e.Message}");
            }
        }

        private async void AnalyzePaaSForMigration_Click(object sender, EventArgs e)
        {
            await AnalyzePaaSForMigration();
        }

        private void UpdateActualPaaSTopologyTreeAndStatistics()
        {
            PaaSRGTreeView.Nodes.Clear();

            if (MigrationPlan.Context.ResourceGroupName == null)
            {
                return;
            }

            var rootNode = new TreeNode();
            PaaSRGTreeView.Nodes.Add(rootNode);

            ExportableReportsCnt = 0;
            AllReportsCnt = 0;

            foreach (var analyzedWSCollection in this.AnalyzedWorkspaceCollections)
            {
                var wscNode = new TreeNode();
                rootNode.Nodes.Add(wscNode);
                wscNode.Tag = analyzedWSCollection;

                analyzedWSCollection.ExportableReportsCnt = 0;
                analyzedWSCollection.AllReportsCnt = 0;

                foreach (var analyzedWS in analyzedWSCollection.Workspaces)
                {
                    analyzedWS.ExportableReportsCnt = analyzedWS.Reports.Where(r => !r.IsPushDataset && !r.IsBoundToOldDataset).Count();

                    analyzedWSCollection.ExportableReportsCnt += analyzedWS.ExportableReportsCnt;
                    analyzedWSCollection.AllReportsCnt += analyzedWS.Reports.Count;

                    var wsNode = new TreeNode($"({analyzedWS.ExportableReportsCnt}/{analyzedWS.Reports.Count}){analyzedWS.WorkspaceId}");
                    wscNode.Nodes.Add(wsNode);
                    wsNode.Tag = analyzedWS;

                    if (analyzedWS.Reports.Count > 0 && analyzedWS.ExportableReportsCnt != analyzedWS.Reports.Count)
                    {
                        wsNode.ForeColor = Color.Red;
                    }
                }

                ExportableReportsCnt += analyzedWSCollection.ExportableReportsCnt;
                AllReportsCnt += analyzedWSCollection.AllReportsCnt;

                wscNode.Text = $"({analyzedWSCollection.ExportableReportsCnt}/{analyzedWSCollection.AllReportsCnt}){analyzedWSCollection.WorkspaceCollectionName}";
                if (analyzedWSCollection.AllReportsCnt > 0 && analyzedWSCollection.ExportableReportsCnt != analyzedWSCollection.AllReportsCnt)
                {
                    wscNode.ForeColor = Color.Red;
                }
            }

            rootNode.Text = $"({ExportableReportsCnt}/{AllReportsCnt}){MigrationPlan.Context.ResourceGroupName}";
            rootNode.Expand();

            UpdateReportStatisticsLabel();
        }

        private void UpdateReportStatisticsLabel(string firstLine, int exportableReportsCnt, int allReportsCnt)
        {
            reportsStatisticsLbl.Text = $"{firstLine}\r\n{exportableReportsCnt} out of {allReportsCnt} reports are downloadable.";
        }

        private void UpdateReportStatisticsLabel()
        {
            UpdateReportStatisticsLabel($"Resource group: {MigrationPlan.Context.ResourceGroupName}", ExportableReportsCnt, AllReportsCnt);
        }

        private void UpdateReportStatisticsLabel(AnalyzedWorkspaceCollection analyzedWorkspaceCollection)
        {
            UpdateReportStatisticsLabel($"Workspace Collection: {analyzedWorkspaceCollection.WorkspaceCollectionName}", analyzedWorkspaceCollection.ExportableReportsCnt,analyzedWorkspaceCollection.AllReportsCnt);
        }

        private void UpdateReportStatisticsLabel(AnalyzedWorkspace analyzedWorkspace)
        {
            UpdateReportStatisticsLabel($"Workspace: {analyzedWorkspace.WorkspaceId}", analyzedWorkspace.ExportableReportsCnt, analyzedWorkspace.Reports.Count);
        }

        private async Task AnalyzeWorkspaceCollection(AnalyzedWorkspaceCollection analyzedWorkspaceCollection)
        {
            PBIProvisioningContext localContext = new PBIProvisioningContext(MigrationPlan.Context);
            localContext.WorkspaceCollection = new PBIWorkspaceCollection { Name = analyzedWorkspaceCollection.WorkspaceCollectionName };
            PaaSController controller = new PaaSController(localContext);

            analyzedWorkspaceCollection.Workspaces = (await controller.GetWorkspaces()).Select(ws => new AnalyzedWorkspace(ws.WorkspaceId)).ToList();

            await Task.WhenAll(analyzedWorkspaceCollection.Workspaces.Select(aws => AnalyzeWorkspace(localContext, aws)).ToArray());
        }

        private async Task AnalyzeWorkspace(PBIProvisioningContext context, AnalyzedWorkspace analyzedWorkspace)
        {
            PBIProvisioningContext localContext = new PBIProvisioningContext(context);
            localContext.WorkspaceId = analyzedWorkspace.WorkspaceId;

            PaaSController controller = new PaaSController(localContext);
            var imports = await controller.GetImports();
            var reports = await controller.GetReports();
            var datasets = await controller.GetDatasets();

            foreach (var report in reports)
            {
                var datasources = await controller.GetDatasources(report.DatasetId);

                var import = imports.Value.FirstOrDefault(i => i.Reports != null && i.Reports.Any(r => string.Equals(r.Id, report.Id, StringComparison.OrdinalIgnoreCase)));

                var dataset = datasets.FirstOrDefault(d => string.Equals(d.Id, report.DatasetId, StringComparison.OrdinalIgnoreCase));

                analyzedWorkspace.Reports.Add(new ReportMigrationData(report, import, dataset, datasources, localContext.WorkspaceCollection.Name, localContext.WorkspaceId));
            }
        }

        private void UsePaaSTopologyForExport()
        {
            HashSet<string> generatedPaths = new HashSet<string>();

            foreach (var report in MigrationPlan.ReportsMigrationData)
            {
                if (!report.IsPushDataset)
                {
                    var path = Path.Combine(
                        rootFolderTb.Text,
                        report.PaaSWorkspaceCollectionName,
                        report.PaaSWorkspaceId,
                        $"{report.PaaSReportName}.pbix");

                    // If such path already exists, then some workspace has two reports with the same name.
                    // Thus, add a report id to the file name of one of the duplicates.
                    if (generatedPaths.Contains(path))
                    {
                        path = Path.Combine(
                        rootFolderTb.Text,
                        report.PaaSWorkspaceCollectionName,
                        report.PaaSWorkspaceId,
                        $"{report.PaaSReportName}-{report.PaaSReportId}.pbix");
                    }

                    report.PbixPath = path;
                    generatedPaths.Add(path);
                }
            }

            UpdateExportTreeView();
        }

        private void UpdateImportToSaaSTreeView()
        {
            importToSaaSTV.Nodes.Clear();

            foreach (var report in AnalyzedReportsSourceManager.FilteredReports)
            {
                if (!report.IsPushDataset)
                {
                    // add GroupNode if required
                    var groupNode = importToSaaSTV.Nodes.Cast<TreeNode>().FirstOrDefault(r => r.Text == report.SaaSTargetGroupName);
                    if (groupNode == null)
                    {
                        groupNode = new TreeNode(report.SaaSTargetGroupName);
                        importToSaaSTV.Nodes.Add(groupNode);
                    }

                    // add Node for Report
                    var reportNode = new TreeNode(report.SaaSTargetReportName);
                    groupNode.Nodes.Add(reportNode);
                    reportNode.Tag = report;
                }
            }
        }

        private void PopulateTreeView(TreeView treeView)
        {
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (var report in AnalyzedReportsSourceManager.FilteredReports)
            {
                var path = report.PbixPath;

                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }
                subPathAgg = string.Empty;
                foreach (string subPath in path.Split('\\'))
                {
                    subPathAgg += subPath + '\\';
                    TreeNode[] nodes = treeView.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = treeView.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                }
                lastNode = null;
            }
        }

        private void UpdateExportTreeView()
        {
            ExportToDiskTV.Nodes.Clear();

            PopulateTreeView(ExportToDiskTV);

            if (ExportToDiskTV.Nodes.Count == 1)
            {
                var node = ExportToDiskTV.Nodes[0];

                // continue recursively
                while (node.Nodes.Count == 1)
                {
                    var eliminated = node.Nodes[0];

                    node.Nodes.Clear();
                    foreach (var child in eliminated.Nodes)
                    {
                        node.Nodes.Add((TreeNode)child);
                    }

                    node.Text = $"{node.Text}\\{eliminated.Text}";
                    node.Expand();
                }
            }
        }

        private void UsePaaSTopologyForImport()
        {
            foreach (var analyzedWSCollection in AnalyzedWorkspaceCollections)
            {
                foreach (var analyzedWS in analyzedWSCollection.Workspaces)
                {
                    foreach (var report in analyzedWS.Reports)
                    {
                        report.SaaSTargetGroupName = $"{groupPrefixTB.Text}-{analyzedWS.WorkspaceId}";
                        report.SaaSTargetReportName = $"{report.PaaSReportName}";

                        report.ResetImportProgressData();
                    }
                }
            }

            UpdateImportToSaaSTreeView();
        }

        private void GenerateAnalyzedWorkspaceCollections()
        {
            AnalyzedWorkspaceCollections = new List<AnalyzedWorkspaceCollection>();

            foreach (var report in MigrationPlan.ReportsMigrationData)
            {
                var analyzedWSCollection = AnalyzedWorkspaceCollections.FirstOrDefault(awc => string.Equals(report.PaaSWorkspaceCollectionName, awc.WorkspaceCollectionName, StringComparison.OrdinalIgnoreCase));
                if (analyzedWSCollection == null)
                {
                    analyzedWSCollection = new AnalyzedWorkspaceCollection(report.PaaSWorkspaceCollectionName);
                    AnalyzedWorkspaceCollections.Add(analyzedWSCollection);
                }

                var analyzedWS = analyzedWSCollection.Workspaces.FirstOrDefault(aws => string.Equals(report.PaaSWorkspaceId, aws.WorkspaceId, StringComparison.OrdinalIgnoreCase));
                if (analyzedWS == null)
                {
                    analyzedWS = new AnalyzedWorkspace(report.PaaSWorkspaceId);
                    analyzedWSCollection.Workspaces.Add(analyzedWS);
                }
                analyzedWS.Reports.Add(report);
            }
        }

        private void PaaSRGTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                AnalyzedReportsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData;
                UpdateReportStatisticsLabel();
            }
            else if (e.Node.Tag is AnalyzedWorkspaceCollection)
            {
                var analyzedWSCollection = e.Node.Tag as AnalyzedWorkspaceCollection;
                AnalyzedReportsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(r => string.Equals(analyzedWSCollection.WorkspaceCollectionName, r.PaaSWorkspaceCollectionName)).ToList();
                UpdateReportStatisticsLabel(analyzedWSCollection);
            }
            else if (e.Node.Tag is AnalyzedWorkspace)
            {
                var analyzedWS = e.Node.Tag as AnalyzedWorkspace;
                AnalyzedReportsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(r => string.Equals(analyzedWS.WorkspaceId, r.PaaSWorkspaceId, StringComparison.OrdinalIgnoreCase)).ToList();
                UpdateReportStatisticsLabel(analyzedWS);
            }

            UpdateAnalyzeGrid();
            UpdateExportTreeView();
            UpdateImportToSaaSTreeView();
        }

        private void UpdateExportGrid()
        {
            ExportSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(
                r => !string.IsNullOrEmpty(r.PaaSReportId) &&
                !r.IsPushDataset &&
                !r.IsBoundToOldDataset
            ).ToList();

            ExportSourceManager.UpdateSource();

            SaveMigrationPlan();
        }

        public void RunInUIContext(Action action)
        {
            MainForm.MainControl.Invoke(action);
        }

        private void UpdateImportGrid()
        {
            ImportSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(r => File.Exists(r.PbixPath)).ToList();

            ImportSourceManager.UpdateSource();

            SaveMigrationPlan();
        }

        private void UpdateConnectionsGrid()
        {
            ConnectionsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData
                .Where(r => !string.IsNullOrWhiteSpace(r.DirectQueryConnectionString))
                .Where(r => r.SaaSImportState == ImportState.Done).ToList();

            ConnectionsSourceManager.UpdateSource();

            SaveMigrationPlan();
        }

        private void UpdateGroupsGrid()
        {
            GroupsSourceManager.FilteredReports = MigrationPlan.ReportsMigrationData.Where(r => File.Exists(r.PbixPath)).GroupBy(r => r.SaaSTargetGroupName).Select(g => g.First()).ToList();

            GroupsSourceManager.UpdateSource();

            SaveMigrationPlan();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    UpdateAnalyzeGrid();
                    break;
                case 1:
                    UpdateExportGrid();
                    break;
                case 2:
                    UpdateGroupsGrid();
                    break;
                case 3:
                    UpdateImportGrid();
                    break;
                case 4:
                    UpdateConnectionsGrid();
                    break;
            }

            SaveMigrationPlan();
        }

        private void usePaasTologyForDiskBtn_Click_1(object sender, EventArgs e)
        {
            if (MigrationPlan.Context.ResourceGroupName == null)
            {
                MessageBox.Show("There is no PaaS Topology to be used");
                return;
            }

            if (MigrationPlan.ReportsMigrationData.Any())
            {
                var dialogResult = MessageBox.Show(
                    $"A download plan already exists. If you have made changes to the download structure, they will be overwritten.\r\n\r\nUse Folder Structure like in current PaaS topology: {rootFolderTb.Text}\\ResourceGroup\\WorkspaceCollection\\Workspace\\reportName.pbix?", "Change download structure", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            UsePaaSTopologyForExport();
            SaveMigrationPlan();
        }

        List<ReportMigrationData> GetSelectedReports(DataGridView grid, List<ReportMigrationData> relevantReports)
        {
            HashSet<ReportMigrationData> selectedReports = new HashSet<ReportMigrationData>();

            if (!relevantReports.Any())
            {
                return relevantReports;
            }

            for (int i = 0; i < grid.SelectedCells.Count; i++)
            {
                selectedReports.Add(relevantReports[grid.SelectedCells[i].RowIndex]);
            }

            var resultList = selectedReports.ToList();
            resultList.Reverse();

            return resultList;
        }

        private async Task Export(List<ReportMigrationData> selectedReports)
        {
            foreach (var report in selectedReports)
            {
                RunInUIContext(UpdateExportGrid);

                PBIProvisioningContext localContext = new PBIProvisioningContext(MigrationPlan.Context);
                localContext.WorkspaceCollection = new PBIWorkspaceCollection { Name = report.PaaSWorkspaceCollectionName };
                PaaSController controller = new PaaSController(localContext);

                string targetFolder = Path.GetDirectoryName(report.PbixPath);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                report.ExportState = ExportState.InProgress;
                RunInUIContext(UpdateExportGrid);

                var response = await TryExportToFile(controller, report.PaaSWorkspaceId, report.PaaSReportId, report.PbixPath);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    if (File.Exists(report.PbixPath))
                    {
                        report.ExportState = ExportState.Done;
                        report.LastExportStatus = "Download was successful";
                    }
                    else
                    {
                        report.ExportState = ExportState.Failed;
                        report.LastExportStatus = Errors.Export.PBIXNotCreated;
                    }
                }
                else
                {
                    report.ExportState = ExportState.Failed;
                    report.LastExportStatus = Errors.Export.DownloadFailed;

                    if (response != null)
                    {
                        report.LastExportStatus = response.StatusCode.ToString();
                    }
                }

                RunInUIContext(UpdateExportGrid);
            }
        }

        private async Task<HttpResponseMessage> TryExportToFile(PaaSController controller, string workspaceId, string reportId, string filePath)
        {
            try
            {
                return (await controller.ExportToFile(workspaceId, reportId, filePath)).Response;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        private async void exportSelectedBtn_Click(object sender, EventArgs e)
        {
            await BlockTabAndRunTask(async () => await Export(GetSelectedReports(exportGridView, ExportSourceManager.FilteredReports)));
        }

        private async void exportAllbtn_Click(object sender, EventArgs e)
        {
            await BlockTabAndRunTask(async () => await Export(ExportSourceManager.FilteredReports));
        }

        private async Task CreateMissingGroups(List<ReportMigrationData> selectedReports)
        {
            if (!selectedReports.Any())
            {
                return;
            }

            var groups = SaaSController.GetGroups();

            foreach (var report in selectedReports)
            {
                report.SaaSTargetGroupCreationStatus = "In Progress";
                RunInUIContext(UpdateGroupsGrid);

                var group = groups.Value.FirstOrDefault(g => string.Equals(g.Name, report.SaaSTargetGroupName, StringComparison.OrdinalIgnoreCase));

                if (group == null)
                {
                    group = await SaaSController.CreateGroupAsync(report.SaaSTargetGroupName);
                    if (group == null)
                    {
                        report.SaaSTargetGroupCreationStatus = Errors.GroupCreation.FailedToCreateGroup;
                        continue;
                    }

                    foreach (var same_group_report in MigrationPlan.ReportsMigrationData.Where(r => string.Equals(r.SaaSTargetGroupName, report.SaaSTargetGroupName, StringComparison.OrdinalIgnoreCase)))
                    {
                        same_group_report.SaaSTargetGroupId = group.Id;
                        same_group_report.SaaSTargetGroupCreationStatus = "Done";
                    }
                }
                else  // group is found by GetGroups
                {
                    report.SaaSTargetGroupCreationStatus = "Done";
                    report.SaaSTargetGroupId = group.Id;
                }
            }
            RunInUIContext(UpdateGroupsGrid);
            SaveMigrationPlan();
        }

        private async void createMissingGroupsBtn_Click(object sender, EventArgs e)
        {
            await BlockTabAndRunTask(async () => await CreateMissingGroups(GroupsSourceManager.FilteredReports));
        }

        private async void createSelectedMissingGroupBtn_Click(object sender, EventArgs e)
        {
            await BlockTabAndRunTask(async () => await CreateMissingGroups(GetSelectedReports(groupsGridView, GroupsSourceManager.FilteredReports)));
        }

        private async void importSelectedBtn_Click(object sender, EventArgs e)
        {
            string nameConflict = nameConflictCB.Text;

            await BlockTabAndRunTask(async () => await Import(GetSelectedReports(importGridView, ImportSourceManager.FilteredReports), nameConflict));
        }

        private async void importAllBtn_Click(object sender, EventArgs e)
        {
            string nameConflict = nameConflictCB.Text;
            await BlockTabAndRunTask(async () => await Import(ImportSourceManager.FilteredReports, nameConflict));
        }

        private async Task Import(List<ReportMigrationData> selectedReports, string selectedNameConflict)
        {
            if (!selectedReports.Any())
            {
                return;
            }

            var groups = SaaSController.GetGroups();

            foreach (var report in selectedReports)
            {
                Microsoft.PowerBI.Api.V2.Models.ODataResponseListImport imports = null;

                if (!string.IsNullOrWhiteSpace(report.SaaSReportId))
                {
                    // The report was already imported. Do nothing.
                    continue;
                }

                // We are ready for import if
                // 1. Get Group retuns the group
                // 2. The group Id is the same as expected
                var group = groups.Value.FirstOrDefault(g => string.Equals(g.Name, report.SaaSTargetGroupName, StringComparison.OrdinalIgnoreCase));

                report.SaaSImportState = ImportState.InProgress;
                RunInUIContext(UpdateImportGrid);

                if (group == null)
                {
                    report.SaaSImportState = ImportState.Failed;
                    report.SaaSImportError = Errors.Import.GroupWasNotFound;
                    continue;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(report.SaaSTargetGroupId))
                    {
                        report.SaaSTargetGroupId = group.Id;
                    }
                    else if (!string.Equals(report.SaaSTargetGroupId, group.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        report.SaaSImportError = Errors.Import.GroupIdsMismatch;
                        report.SaaSImportState = ImportState.Failed;
                        continue;
                    }
                }

                imports = await SaaSController.GetImports(report.SaaSTargetGroupId);

                string nameConflict = null;

                var existing_reports = await SaaSController.GetReports(report.SaaSTargetGroupId);
                if (existing_reports != null && existing_reports.Value.Where(r => string.Equals(r.Name, report.SaaSTargetReportName, StringComparison.OrdinalIgnoreCase)).Any())
                {
                    // report with the same name exists
                    if (selectedNameConflict == "Abort")
                    {
                        report.SaaSImportError = Errors.Import.DuplicateReport;
                        report.SaaSImportState = ImportState.Failed;
                        RunInUIContext(UpdateImportGrid);
                        continue;
                    }
                    else
                    {
                        nameConflict = selectedNameConflict;
                    }
                }
                else // report with the same name does not exist
                {
                    nameConflict = "Abort";
                }

                report.SaaSImportState = ImportState.Publishing;
                RunInUIContext(UpdateImportGrid);
                var importId = await TrySendImport(report.PbixPath, report.SaaSTargetGroupId, report.SaaSTargetReportName, nameConflict);

                if (importId == null)
                {
                    report.SaaSImportError = Errors.Import.UploadFailed;
                    report.SaaSImportState = ImportState.Failed;
                    RunInUIContext(UpdateImportGrid);
                    continue;
                }

                // polling
                Microsoft.PowerBI.Api.V2.Models.Import import = null;
                do
                {
                    imports = await SaaSController.GetImports(report.SaaSTargetGroupId);
                    if (imports != null)
                    {
                        import = imports.Value.FirstOrDefault(i => string.Equals(i.Id, importId, StringComparison.OrdinalIgnoreCase));
                    }
                } while (import == null || (import.ImportState != "Succeeded" && import.ImportState != "Failed"));

                report.SaaSImportState = import.ImportState == "Succeeded" ? ImportState.Done : ImportState.Failed;
                if (import.ImportState == "Succeeded" && import.Reports.Count == 1)
                {
                    report.SaaSReportId = import.Reports[0].Id;
                    report.SaaSImportError = null;
                }
                else
                {
                    report.SaaSImportError = Errors.Import.UploadFailed;
                }
            }

            RunInUIContext(UpdateImportGrid);
        }

        private async Task<string> TrySendImport(string filePath, string groupId, string targetReportName,string  nameConflict)
        {
            try
            {
                return await SaaSController.SendImport(filePath, groupId, targetReportName, nameConflict);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        private void resetGroupIds_click(object sender, EventArgs e)
        {
            foreach (var report in MigrationPlan.ReportsMigrationData)
            {
                report.SaaSTargetGroupId = null;
            }
            UpdateGroupsGrid();
        }

        private void ResetAllBtn_Click(object sender, EventArgs e)
        {
            ResetImportIds(ImportSourceManager.FilteredReports);
        }

        private void ResetImportIds(List<ReportMigrationData> selectedReports)
        {
            foreach (var report in selectedReports)
            {
                report.SaaSReportId = null;
            }
            UpdateImportGrid();
        }

        private void resetSelectedBtn_Click(object sender, EventArgs e)
        {
            ResetImportIds(GetSelectedReports(importGridView, ImportSourceManager.FilteredReports));
        }

        private void rootDirTB_TextChanged(object sender, EventArgs e)
        {
            MigrationPlan.MigrationRootPath = rootFolderTb.Text;
        }

        private void UsePaaSTopologyForSaaSBtn_Click(object sender, EventArgs e)
        {
            if (MigrationPlan.Context.ResourceGroupName == null)
            {
                MessageBox.Show("There is no PaaS Topology to be used");
                return;
            }

            if (MigrationPlan.ReportsMigrationData.Any())
            {
                if (!PromptImportPlanOverWrite())
                {
                    return;
                }
            }
            else
            {
                return;
            }

            UsePaaSTopologyForImport();
        }

        private bool PromptImportPlanOverWrite()
        {
            var dialogResult = MessageBox.Show(
                "An upload plan already exists. If you have made changes to the target structure, they will be overwritten.\r\n\r\nIf you have already created groups and uploaded reports, GroupIDs and UploadedReportIDs will also be overwritten.\r\n\r\nAre you sure you want to continue and overwrite this data?",
                "Change target structure", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);

            return dialogResult == DialogResult.Yes;
        }

        private void analyzeGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var cell = analyzeGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewLinkCell;

            if (cell != null)
            {
                var val = cell.Value as string;

                if (val == AnalyzeSourceManager.ReportTypeStrings.PushDataset)
                {
                    Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-code-snippets/#push-dataset-amp-report");
                }
                else if (val == AnalyzeSourceManager.ReportTypeStrings.OldImport)
                {
                    Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#upload-local-file");
                }
                else if (val == AnalyzeSourceManager.ReportTypeStrings.ImportedDirectQuery)
                {
                    Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#directquery-reports");
                }
                else if (val == AnalyzeSourceManager.ReportTypeStrings.ImportedCached)
                {
                    Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#step-2-download");
                }
            }
        }

        private void importToSaaSTV_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // if this is a group node
                if (e.Node.Parent == null)
                {
                    ContextMenuStrip contexMenu = new ContextMenuStrip();

                    foreach (var action in new List<string> { MigrationPlanActions.RenameGroup})
                    {
                        contexMenu.Items.Add(action).Tag = e.Node.Text;
                    }

                    contexMenu.Show(Cursor.Position);
                    contexMenu.ItemClicked += new ToolStripItemClickedEventHandler(ImportPlanMenu_ItemClicked);
                }
            }
        }

        private void ImportPlanMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            // hide the context menu
            var contextMenu = sender as ContextMenuStrip;
            contextMenu.Visible = false;

            if (item.Text == MigrationPlanActions.RenameGroup)
            {
                var prevGroupName = item.Tag as string;

                if (!PromptImportPlanOverWrite())
                {
                    return;
                }

                var renameForm = new RenameForm(prevGroupName);
                renameForm.Text = "Rename Group";

                var dialogResult = renameForm.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                var newGroupName = renameForm.Result;

                if (string.IsNullOrWhiteSpace(newGroupName))
                {
                    MessageBox.Show($"Cannot rename group  to {newGroupName}", "Migration Sample", MessageBoxButtons.OK,  MessageBoxIcon.Warning);
                }

                if (string.Equals(newGroupName, prevGroupName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                foreach (var report in MigrationPlan.ReportsMigrationData.Where(r => r.SaaSTargetGroupName == prevGroupName))
                {
                    report.SaaSTargetGroupName = newGroupName;
                    report.ResetImportProgressData();
                }

                UpdateImportToSaaSTreeView();
            }
        }

        private void nextToDownloadBtn_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 1;
        }

        private void prevToAnalyze_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 0;
        }

        private void nextToCreateGroupsBtn_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 2;
        }

        private void prevToDownloadBtn_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 1;
        }

        private void nextToUploadBtn_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 3;
        }

        private void prevToCreatGroupBtn_Click(object sender, EventArgs e)
        {
            mainMigrationTc.SelectedIndex = 2;
        }

        private void exportGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (exportGridView.RowCount > 1 && e.ColumnIndex == exportGridView.Columns["LastExportStatus"].Index && e.Value != null)
            {
                DataGridViewCell cell = exportGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (e.Value.Equals(Errors.Export.PBIXNotCreated))
                {
                    cell.Style.ForeColor = Color.Red;
                    cell.ToolTipText = "Download was successful, but a PBIX file was not created.\r\nCheck that the report is closed and that there is sufficient disc space, and try again.";
                }
                else if (e.Value.Equals(Errors.Export.DownloadFailed))
                {
                    cell.Style.ForeColor = Color.Red;
                    cell.ToolTipText = "There was a problem downloading this report.\r\nCheck that the report still exists and was created after November 26, 2016.";
                }
            }
        }

        private void groupsGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (groupsGridView.RowCount > 1 && groupsGridView.RowCount > 0 && e.ColumnIndex == groupsGridView.Columns["GroupCreationStatus"].Index && e.Value != null)
            {
                DataGridViewCell cell = groupsGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (e.Value.Equals(Errors.GroupCreation.FailedToCreateGroup))
                {
                    cell.Style.ForeColor = Color.Red;
                }
            }
        }

        private void importGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (importGridView.RowCount > 1 && e.ColumnIndex == importGridView.Columns["UploadError"].Index && e.Value != null)
            {
                DataGridViewCell cell = this.importGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (e.Value.Equals(Errors.Import.GroupIdsMismatch))
                {
                    cell.Style.ForeColor = Color.Red;
                    cell.ToolTipText = "The GroupID you are trying to upload is different from the GroupID on the service.";
                }
                else if (e.Value.Equals(Errors.Import.DuplicateReport))
                {
                    cell.Style.ForeColor = Color.Red;
                    cell.ToolTipText = "A report with the same name already exists.";
                }
            }
        }

        private void analyzeGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (analyzeGridView.RowCount > 1 && e.ColumnIndex == analyzeGridView.Columns["Downloadable"].Index && e.Value != null)
            {
                DataGridViewCell cell = analyzeGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!e.Value.Equals(AnalyzeSourceManager.DownloadabilityStrings.CanBeDownloaded))
                {
                    cell.Style.ForeColor = Color.Red;
                }
                else
                {
                    cell.Style.ForeColor = Color.Black;
                }
            }
        }
    }
}
