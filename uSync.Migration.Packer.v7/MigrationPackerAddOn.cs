using Jumoo.uSync.BackOffice.Controllers;

using Umbraco.Web;

namespace uSync.Migration.Packer.v7
{
    /// <summary>
    ///  thing that makes the Migrations tab appear in the backoffice.
    /// </summary>
    public class MigrationPackerAddOn : IuSyncAddOn, IuSyncTab
    {
        public BackOfficeTab GetTabInfo()
        {
            return new BackOfficeTab
            {
                name = "Migrations",
                template = UriUtility.ToAbsolute("/App_Plugins/uSyncPacker/dashboard.html")
            };
        }

        public string GetVersionInfo()
            => $"Migration.Packer: {typeof(MigrationPackerAddOn).Assembly.GetName().Version.ToString(3)}";
    }
}
