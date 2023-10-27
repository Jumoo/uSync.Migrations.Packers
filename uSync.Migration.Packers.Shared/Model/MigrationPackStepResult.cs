using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migration.Packers.Shared.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MigrationPackStepResult
    {
        public MigrationPackStepResult(Guid id, bool complete)
        {
            Id = id;
            Complete = complete;
        }

        public Guid Id { get; set; }
        public bool Complete { get; set; }
    }
}
