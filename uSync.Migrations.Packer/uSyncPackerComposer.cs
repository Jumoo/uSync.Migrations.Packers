using Umbraco.Core;
using Umbraco.Core.Composing;

using uSync.Migrations.Packer.Services;

namespace uSync.Migrations.Packer
{
    public class uSyncPackerComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<MigrationPackService>();
        }
    }
}
