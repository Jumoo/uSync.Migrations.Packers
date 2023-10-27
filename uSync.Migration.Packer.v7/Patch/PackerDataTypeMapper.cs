using System.Collections.Generic;
using System.Linq;

using Jumoo.uSync.Core.Mappers;

using Newtonsoft.Json;

using Umbraco.Core;
using Umbraco.Core.Logging;

namespace uSync.Migration.Packer.v7.Patch
{
    public class PackerDataTypeMapper
         : IContentMapper
    {
        public PackerDataTypeMapper()
        {
            LogHelper.Debug<PackerDataTypeMapper>("[PACKER] Loaded custom type");
        }

        public string GetExportValue(int dataTypeDefinitionId, string value)
        {
            LogHelper.Debug<PackerDataTypeMapper>("[PACKER] GetExport Value {0}", () => value ?? "");

            var trimmedValue = value.Trim();
            if (!trimmedValue.DetectIsJson()) return value;


            var prevalues = ApplicationContext.Current.Services.
                DataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinitionId)
                .PreValuesAsDictionary;

            var values = JsonConvert.DeserializeObject<IEnumerable<string>>(trimmedValue);
            var results = new List<string>();
            foreach (var item in values)
            {
                if (int.TryParse(item, out int intValue))
                {
                    // lookup int value
                    var preValue = prevalues.Where(kvp => kvp.Value.Id == intValue)
                        .Select(x => x.Value).FirstOrDefault();

                    if (preValue != null)
                    {
                        results.Add(preValue.Value);
                        continue;
                    }
                }

                results.Add(item);
            }

            LogHelper.Debug<PackerDataTypeMapper>("Returning Export Value: {0}", () => JsonConvert.SerializeObject(results, Formatting.Indented));

            return JsonConvert.SerializeObject(results, Formatting.Indented);


        }

        public string GetImportValue(int dataTypeDefinitionId, string content)
        {
            LogHelper.Debug<PackerDataTypeMapper>("[PACKER] DataType: {0} {1}", () => dataTypeDefinitionId, () => content);

            var prevalues = ApplicationContext.Current.Services.
                DataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDefinitionId)
                .PreValuesAsDictionary;

            if (prevalues != null && prevalues.Count > 0)
            {
                var values = content.ToDelimitedList();
                var mapped = new List<string>();

                foreach (var value in values)
                {
                    var preValue = prevalues.Where(kvp => kvp.Value.Value.InvariantEquals(value))
                        .Select(x => x.Value).SingleOrDefault();

                    if (preValue != null)
                    {
                        LogHelper.Debug<ContentDataTypeMapper>("Matched PreValue: [{0}] {1}", () => preValue.Id, () => preValue.Value);
                        mapped.Add(preValue.Id.ToString());
                    }
                    else
                    {
                        LogHelper.Debug<ContentDataTypeMapper>("No Matched Value: {0}", () => value);
                        mapped.Add(value);
                    }
                }

                return string.Join(",", mapped);
            }

            return content;
        }
    }
}
