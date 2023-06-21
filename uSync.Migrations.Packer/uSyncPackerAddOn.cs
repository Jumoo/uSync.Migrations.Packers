using Umbraco.Web;

using uSync8.BackOffice.Models;

namespace uSync.Migrations.Packer
{
    public class uSyncPackerAddOn : ISyncAddOn
    {
        public string Alias => "packer";

        public string Name => "Packer";

        public string Version => typeof(uSyncPackerAddOn).Assembly.GetName().Version.ToString(3);

        public string Icon => "icon-zip color-indigo";

        public string View => UriUtility.ToAbsolute("/App_plugins/uSyncPacker/dashboard.html");

        public string DisplayName => "Packer";

        public int SortOrder => 54;
    }
}
