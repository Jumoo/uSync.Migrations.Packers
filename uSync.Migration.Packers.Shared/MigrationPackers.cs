using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migration.Packers.Shared
{
    internal class MigrationPackers
    {
        public const string SiteFolder = "_site";

#if NET452
        public const string uSyncFolder = "data";
        public const string uSyncContentType = "Jumoo.uSync.Content.ContentEdition";
#else
        public const string uSyncFolder = "v8";
        public const string uSyncContentType = "uSync8.ContentEdition.uSyncContent";
#endif
    }
}
