using System.IO;
using System.IO.Compression;
using System.Linq;

using Newtonsoft.Json;

using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;

using uSync8.BackOffice;
using uSync8.BackOffice.SyncHandlers;

namespace uSync.Migrations.Packer.Services
{
    public class MigrationPackService
    {
        private readonly uSyncService _uSyncService;
        private readonly IGridConfig _gridConfig;

        private const string siteFolder = "_site";
        private const string uSyncFolder = "v8";

        private string _root;

        public MigrationPackService(uSyncService uSyncService = null)
        {
            _root = IOHelper.MapPath("~/uSync/MigrationPacks");
            _uSyncService = uSyncService;
            _gridConfig = Current.Configs.Grids();
        }

        public MemoryStream PackExport()
        {
            var id = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var folder = GetExportFolder(id);

            CreateExport(Path.Combine(folder, uSyncFolder));

            GetGridConfig(folder);

            CopyViews(folder);

            CopyCss(folder);
            CopyScripts(folder);

            var stream = ZipFolder(folder);

            CleanFolder(folder);

            return stream;


        }

        /// <summary>
        ///  get the folder we are going to export into. 
        /// </summary>
        private string GetExportFolder(string id)
            => Path.Combine(_root, id);

        private void CreateExport(string folder)
        {
            var options = new SyncHandlerOptions("default", HandlerActions.Export);
            _ = _uSyncService.Export(folder, options, null);
        }

        private void GetGridConfig(string folder)
        {
            var configJson = JsonConvert.SerializeObject(_gridConfig, Formatting.Indented);
            var configFolder = Path.Combine(folder, siteFolder, "config");

            Directory.CreateDirectory(configFolder);

            var configFile = Path.Combine(configFolder, "grid.editors.config.js");
            File.WriteAllText(configFile, configJson);                
        }

        private void CopyViews(string folder)
            => CopyFolder(IOHelper.MapPath("~/views"), Path.Combine(folder, siteFolder, "views"));

        private void CopyCss(string folder)
            => CopyFolder(IOHelper.MapPath("~/css"), Path.Combine(folder, siteFolder, "css"));
        
        private void CopyScripts(string folder)
            => CopyFolder(IOHelper.MapPath("~/scripts"), Path.Combine(folder, siteFolder, "scripts"));

        private void CopyFolder(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(sourceFolder)) return;

            var files = Directory.GetFiles(sourceFolder, "*.*");
            Directory.CreateDirectory(targetFolder);

            foreach(var file in files)
            {
                var target = Path.Combine(targetFolder, Path.GetFileName(file));
                File.Copy(file, target);    
            }

            foreach(var folder in Directory.GetDirectories(sourceFolder))
            {
                CopyFolder(folder, Path.Combine(targetFolder, Path.GetFileName(folder)));
            }
        }

        /// <summary>
        ///  zip the folder up into return a stream 
        /// </summary>
        private MemoryStream ZipFolder(string folder)
        {
            var folderInfo = new DirectoryInfo(folder);
            var files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var relativePath = file.FullName.Substring(folder.Length + 1);
                    archive.CreateEntryFromFile(file.FullName, relativePath);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private void CleanFolder(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // it can be locked, and this will throw. 
            }
        }
    }
}

