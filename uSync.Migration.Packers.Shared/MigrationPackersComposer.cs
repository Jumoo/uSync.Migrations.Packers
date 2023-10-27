#if NET472
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace uSync.Migration.Packers.Shared
{
    internal class MigrationPackersComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<MigrationPackService>();
        }
    }
}
#endif
