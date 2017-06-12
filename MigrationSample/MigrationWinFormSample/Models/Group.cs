using System.Collections.Generic;

namespace MigrationSample.Core
{
    public class O365Group
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string MailNickname { get; set; }

        public string Visibility { get; set; }

        public bool MailEnabled { get; set; }

        public bool SecurityEnabled { get; set; }

        public List<string> GroupTypes { get; set; }
    }
}
