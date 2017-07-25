using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MigrationSample.Core;
using MigrationSample.Dialogs;

namespace MigrationSample
{
    public partial class MainForm : Form
    {
        private class MenuItemActions {
            public const string DeleteTab = "Delete Tab";
            public const string RenameTab = "Rename Tab";
        }

        public static Form MainControl { get; private set; }

        private SelectResourceGroup SelectResourceGroupDialog { get; set; }

        public MainForm()
        {
            InitializeComponent();

            MainControl = this;

            LoadAppData();
        }

        public ResourceGroupTabPage AddTab(MigrationPlan migrationPlan, string migrationPlanFilePath)
        {
            var tabForm = new MigrationTabForm(migrationPlan, migrationPlanFilePath);
            var page = new ResourceGroupTabPage(tabForm);

            AddPage(page, tabForm.DisplayName);

            return page;
        }

        public ResourceGroupTabPage AddTab(PBIProvisioningContext context)
        {
            var page = new ResourceGroupTabPage(new MigrationTabForm(context));

            AddPage(page, context.DisplayName);

            return page;
        }

        private void AddPage(ResourceGroupTabPage page, string displayText)
        {
            this.tabControlMain.TabPages.Add(page);
            page.Text = displayText;

            this.tabControlMain.SelectedIndex = this.tabControlMain.TabCount - 1;

            page.BackColor = Color.PowderBlue;
        }

        private void SaveTabsToAppData()
        {
            var migrationPlanFiles = new List<string>();

            foreach (var page in this.tabControlMain.TabPages)
            {
                string path = (page as ResourceGroupTabPage).GetMigrationPlanFile();
                if (path != null)
                {
                    migrationPlanFiles.Add(path);
                }
            }

            MigrationWinFormSample.Properties.Settings.Default.Tabs = JsonConvert.SerializeObject(migrationPlanFiles);
            MigrationWinFormSample.Properties.Settings.Default.OpenedTab = tabControlMain.SelectedIndex;
            MigrationWinFormSample.Properties.Settings.Default.Save();
        }

        private void LoadAppData()
        {
            var settingsPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            if (!File.Exists(settingsPath))
            {
                return;
            }

            AzureTokenManager.LoadAppData();

            var filePaths =
                JsonConvert.DeserializeObject<List<string>>(MigrationWinFormSample.Properties.Settings.Default.Tabs);

            if (filePaths == null || !filePaths.Any())
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"The file {filePath} does not exist", "Failed to read Migration Plan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    TryAddTab(filePath);
                }
            }

            tabControlMain.SelectedIndex = MigrationWinFormSample.Properties.Settings.Default.OpenedTab;
        }

        private void TryAddTab(string filePath)
        {
            var migrationPlan = MigrationPlan.Load(filePath);

            if (migrationPlan == null)
            {
                MessageBox.Show($"Failed to read Migration Plan from: {filePath}");
                return;
            }

            AddTab(migrationPlan, filePath);
        }

        /// <summary>
        /// When user right clicks one of the tabs in the tabControlMain, show context menu options
        /// current options are: Delete Tab and Rename Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlMain_MouseUp(object sender, MouseEventArgs e)
        {
            // check if the right mouse button was pressed
            if (e.Button == MouseButtons.Right)
            {
                // iterate through all the tab pages
                for (int i = 0; i < tabControlMain.TabCount; i++)
                {
                    // get their rectangle area and check if it contains the mouse cursor
                    Rectangle r = tabControlMain.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        ContextMenuStrip contexMenu = new ContextMenuStrip();

                        foreach (var action in new List<string>{
                            MenuItemActions.DeleteTab,
                            MenuItemActions.RenameTab
                        })
                        {
                            contexMenu.Items.Add(action).Tag = tabControlMain.TabPages[i];
                        }

                        contexMenu.Show(this, new Point(r.Right + this.tabControlMain.Left, r.Bottom + this.tabControlMain.Top));
                        contexMenu.ItemClicked += new ToolStripItemClickedEventHandler(contextMenuStrip1_ItemClicked);
                    }
                }
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            var page = item.Tag as ResourceGroupTabPage;

            if (string.Equals(item.Text, MenuItemActions.DeleteTab))
            {
                tabControlMain.TabPages.Remove(page);
            }
            else if (string.Equals(item.Text, MenuItemActions.RenameTab))
            {
                var renameForm = new RenameForm(page.Text);
                renameForm.Text = "Rename Tab Form";

                var dialogResult = renameForm.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                var result = renameForm.Result;

                page.SetDisplayName(result);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectResourceGroupDialog == null)
            {
                SelectResourceGroupDialog = new SelectResourceGroup();
            }

            SelectResourceGroupDialog.TargetTabPage = null;

            SelectResourceGroupDialog.ShowDialog(this);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AzureTokenManager.SetAppData();

            SaveTabsToAppData();

            MigrationWinFormSample.Properties.Settings.Default.Save();
        }

        private void personalSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

            if (!File.Exists(path))
            {
                MessageBox.Show($"Configuration Directory {path} has not been created yet. It is created and updated when the Application is closed.");
                return;
            }

            Process.Start(Path.GetDirectoryName(path));
        }

        private string SelectFileToOpen(string initialDirectory = null)
        {
            OpenFileDialog chooseFileDialog = new OpenFileDialog();
            chooseFileDialog.Filter = "XML file|*.xml";
            chooseFileDialog.FilterIndex = 1;
            chooseFileDialog.Title = "Select Migration Plan File";
            chooseFileDialog.Multiselect = false;

            if (initialDirectory != null)
            {
                chooseFileDialog.InitialDirectory = initialDirectory;
            }

            if (chooseFileDialog.ShowDialog() == DialogResult.OK)
            {
                return chooseFileDialog.FileName;
            }

            return null;
        }

        private void openMigrationPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filePath = SelectFileToOpen();

            if (filePath != null)
            {
                TryAddTab(filePath);
            }
        }

        public static string SelectFileToSave(string initDirectory, string defaultFileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML file|*.xml";
            saveFileDialog.Title = "Select Migration Plan File";
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.InitialDirectory = initDirectory;
            saveFileDialog.FileName = defaultFileName;
            var result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

        private void openMigrationRootFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string migrationRoot = ConfigurationManager.AppSettings["MigrationRoot"];

            if (!File.Exists(migrationRoot))
            {
                MessageBox.Show($"Migration Root {migrationRoot} has not been created yet.");
            }

            Process.Start(ConfigurationManager.AppSettings["MigrationRoot"]);
        }

        private void saveMigrationPlanToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.tabControlMain.SelectedTab == null)
            {
                MessageBox.Show("There is no Migration Plan to save");
                return;
            }

            var page = (ResourceGroupTabPage)this.tabControlMain.SelectedTab;

            page.SaveMigrationPlan();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.tabControlMain.SelectedTab == null)
            {
                MessageBox.Show("There is no Migration Plan to save");
                return;
            }

            var page = (ResourceGroupTabPage)this.tabControlMain.SelectedTab;

            page.SaveMigrationPlanAs();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileMenu = (sender as ToolStripMenuItem);

            bool enableSave = tabControlMain.TabPages.Count != 0;

            fileMenu.DropDownItems[2].Enabled = enableSave;
            fileMenu.DropDownItems[3].Enabled = enableSave;
        }

        private void analyzePlanMigrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#step-1-analyze-amp-plan-migration");
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#step-2-download");
        }

        private void createGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#step-3-create-groups");
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool/#step-4-upload");
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://powerbi.microsoft.com/documentation/powerbi-developer-migrate-tool");
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            foreach (var page in tabControlMain.TabPages)
            {
                ((ResourceGroupTabPage)page).ResizeItemSize();
            }
        }
    }
}
