using System.Windows.Forms;

namespace MigrationSample.Dialogs
{
    public class ResourceGroupTabPage : TabPage
    {
        private MigrationTabForm migrationTabForm { get; set; }

        public ResourceGroupTabPage(MigrationTabForm form)
        {
            this.migrationTabForm = form;
            this.Controls.Add(form.flowLayoutPanel);
            this.Text = form.Text;
        }

        public void SetContext(PBIProvisioningContext context)
        {
            this.Text = context.DisplayName;
        }

        public void SetDisplayName(string name)
        {
            migrationTabForm.MigrationPlan.Context.DisplayName = name;
            Text = name;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { migrationTabForm.Dispose(); }
            base.Dispose(disposing);
        }

        public string GetMigrationPlanFile()
        {
            return migrationTabForm.GetMigrationPlanFile();
        }

        public void SaveMigrationPlan()
        {
            migrationTabForm.SaveMigrationPlan();
        }

        public void ResizeItemSize()
        {
            migrationTabForm.ResizeItemSize();
        }

        public void SaveMigrationPlanAs()
        {
            migrationTabForm.SaveMigrationPlanAs();
        }
    }

    public class ProvisioningFormPage : Form
    {
        public TableLayoutPanel flowLayoutPanel;
    }
}
