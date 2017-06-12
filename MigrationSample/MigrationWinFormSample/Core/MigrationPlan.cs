using System.Collections.Generic;
using System.Xml.Serialization;

namespace MigrationSample.Core
{
    public class MigrationPlan
    {
        public List<ReportMigrationData> ReportsMigrationData { get; set; }

        public PBIProvisioningContext Context { get; set; }

        public string MigrationRootPath { get; set; }

        public MigrationPlan()
        {
        }

        public MigrationPlan(PBIProvisioningContext context)
        {
            Context = context;

            ReportsMigrationData = new List<ReportMigrationData>();
        }

        /// <summary>
        /// Saves to an xml file
        /// </summary>
        /// <param name="FileName">File path of the new xml file</param>
        public void Save(string FileName)
        {
            using (var writer = new System.IO.StreamWriter(FileName))
            {
                var serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }

        /// <summary>
        /// Load an object from an xml file
        /// </summary>
        /// <param name="FileName">Xml file name</param>
        /// <returns>The object created from the xml file</returns>
        public static MigrationPlan Load(string FileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenRead(FileName))
                {
                    var serializer = new XmlSerializer(typeof(MigrationPlan));
                    return serializer.Deserialize(stream) as MigrationPlan;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
