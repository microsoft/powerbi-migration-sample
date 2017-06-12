using Newtonsoft.Json;
using MigrationSample.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigrationSample.Dialogs
{
    public partial class SelectResourceGroup : Form
    {
        public ResourceGroupTabPage TargetTabPage { get; set; }

        public SelectResourceGroup()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var context = GetContext();

            if (context.Subscription == null)
            {
                MessageBox.Show("Select a Subscription");
                return;
            }

            if (context.ResourceGroupName == null)
            {
                MessageBox.Show("Select a resource group");
                return;
            }

            this.Close();

            context.DisplayName = $"{context.Subscription.DisplayName}-{context.ResourceGroupName}";

            if (TargetTabPage == null)
            {
                MainForm parent = (MainForm)this.Owner;
                TargetTabPage = parent.AddTab(context);
            }
            else {
                TargetTabPage.SetContext(context);
            }
        }

        private async void comboEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            MigrationWinFormSample.Properties.Settings.Default.Environment = comboEnvironment.SelectedIndex;

            await BlockUIAndExecuteAsync(LoadSubscriptions);
        }

        public async Task LoadSubscriptions()
        {
            this.SubscriptionsGridView.DataSource = null;
            this.comboResourceGroup.DataSource = null;

            PaaSController backend = new PaaSController(GetContext());

            try
            {
                string content = await backend.GetSubscriptions();
                AzureSubscriptions subscriptions = JsonConvert.DeserializeObject<AzureSubscriptions>(content);
                if (subscriptions == null || subscriptions.Value == null || subscriptions.Value.Count() == 0)
                {
                    MessageBox.Show("No subscriptions found.");
                    return;
                }
                this.SubscriptionsGridView.DataSource = subscriptions.Value;
                this.SubscriptionsGridView.CurrentCell = this.SubscriptionsGridView[1, 0];
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to get subscriptions: {e.Message}");
                return;
            }

            await LoadResourceGroups();
        }

        public PBIProvisioningContext GetContext()
        {
            MigrationEnvironment env;
            if (!Enum.TryParse(this.comboEnvironment.SelectedItem as string, out env))
            {
                throw new Exception("Not supported Environement.The app should bail out.");
            }

            Subscription subscription = null;
            if (SubscriptionsGridView.CurrentCell != null)
            {
                subscription = SubscriptionsGridView.Rows[SubscriptionsGridView.CurrentCell.RowIndex].DataBoundItem as Subscription;
            }

            return new  PBIProvisioningContext()
            {
                Environment = env,
                Subscription = subscription,
                ResourceGroupName = comboResourceGroup.SelectedItem as string,
            };
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            await BlockUIAndExecuteAsync(LoadResourceGroups);
        }

        public async Task LoadResourceGroups()
        {
            PaaSController backend = new PaaSController(GetContext());

            string content = await backend.GetResourceGroups();
            AzureResourceGroups resourceGroups = JsonConvert.DeserializeObject<AzureResourceGroups>(content);
            if (resourceGroups == null || resourceGroups.Value == null || resourceGroups.Value.Count() == 0)
            {
                MessageBox.Show("No subscriptions found.");
                return;
            }

            this.comboResourceGroup.DataSource = resourceGroups.Value.Select(rg => rg.Name).ToList();
        }

        public async Task BlockUIAndExecuteAsync(Func<Task> func)
        {
            this.Enabled = false;
            try
            {
                await func();
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async void SubscriptionsGridView_MouseUp(object sender, MouseEventArgs e)
        {
            await BlockUIAndExecuteAsync(LoadResourceGroups);
        }

        private void SelectResourceGroup_Load(object sender, EventArgs e)
        {
            var defaultEnvironementString = ConfigurationManager.AppSettings["default-environement"];
            MigrationEnvironment defaultEnvironement;

            if (!string.IsNullOrWhiteSpace(defaultEnvironementString) && Enum.TryParse(defaultEnvironementString, out defaultEnvironement))
            {
                comboEnvironment.Items.Add(defaultEnvironement.ToString());
                comboEnvironment.SelectedIndex = 0;
                comboEnvironment.Enabled = false;
            }
            else
            {
                comboEnvironment.Items.Clear();
                foreach (MigrationEnvironment env in Enum.GetValues(typeof(MigrationEnvironment)))
                {
                    comboEnvironment.Items.Add(env.ToString());
                }
            }
        }
    }
}
