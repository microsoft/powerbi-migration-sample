using MigrationSample.Models;

namespace MigrationSample
{
    public enum MigrationEnvironment
    {
        dxt,
        msit,
        prod
    };

    public class PBIProvisioningContext
    {
        public string DisplayName;

        public MigrationEnvironment Environment { get; set; }

        public Subscription Subscription { get; set; }

        public string ResourceGroupName { get; set; }

        public PBIWorkspaceCollection WorkspaceCollection { get; set; }

        public string WorkspaceId { get; set; }

        public PBIProvisioningContext()
        {

        }

        public PBIProvisioningContext(PBIProvisioningContext previousContext)
        {
            DisplayName = previousContext.DisplayName;
            Environment = previousContext.Environment;
            Subscription = previousContext.Subscription;
            ResourceGroupName = previousContext.ResourceGroupName;
            WorkspaceCollection = previousContext.WorkspaceCollection;
            WorkspaceId = previousContext.WorkspaceId;
        }
    }
}

