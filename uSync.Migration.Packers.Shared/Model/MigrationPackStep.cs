using System;

namespace uSync.Migration.Packers.Shared.Model
{
    public class MigrationPackStep
    {
        public MigrationPackStep(string name, Func<Guid, Guid> action)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; set; }

        public Func<Guid, Guid> Action { get; set; }
    }
}
