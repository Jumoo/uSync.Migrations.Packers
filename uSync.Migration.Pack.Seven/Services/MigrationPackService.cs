using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Jumoo.uSync.BackOffice;

using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.IO;
using Newtonsoft.Json;

namespace uSync.Migration.Pack.Seven.Services
{
    internal class MigrationPackService
    {
        private const string siteFolder = "_site";
        private const string uSyncFolder = "data";

        private string _root;

        public MigrationPackService()
        {
            _root = IOHelper.MapPath("~/uSync/MigrationPacks");
        }

        public FileStream PackExport()
        {
            var id = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var folder = GetExportFolder(id);

            // grab the things we want to include in the pack 

            // a full uSync export
            CreateExport(Path.Combine(folder, uSyncFolder));

            // the grid.config for the site
            GetGridConfig(folder);

            // views
            CopyViews(folder);

            // css and scripts
            CopyCss(folder);
            CopyScripts(folder);

            // make the stream
            var stream = ZipFolder(folder);

            // clean the folder we used
            CleanFolder(folder);

            return stream;
        }

        /// <summary>
        /// Get uSync to do a full export. 
        /// </summary>
        private void CreateExport(string folder)
        {
            _ = uSyncBackOfficeContext.Instance.ExportAll(folder);
        }

        private void GetGridConfig(string folder)
        {
            var appPlugins = "..\\App_Plugins";
            var configFolder = "..\\Config";
            var debugging = false;

            var gridConfig = UmbracoConfig.For.GridConfig(
                ApplicationContext.Current.ProfilingLogger.Logger,
                ApplicationContext.Current.ApplicationCache.RuntimeCache,
                new DirectoryInfo(appPlugins),
                new DirectoryInfo(configFolder),
                debugging);


            var configJson = JsonConvert.SerializeObject(gridConfig.EditorsConfig.Editors, Formatting.Indented);

            var configFile = Path.Combine(folder, siteFolder, "config", "grid.editors.config.js");

            Directory.CreateDirectory(Path.GetDirectoryName(configFile));   
              
            File.WriteAllText(configFile, configJson);
        }

        private void CopyViews(string folder)
        {
            var viewsRoot = IOHelper.MapPath("~/views");
            var viewsTarget = Path.Combine(folder, siteFolder, "views");

            CopyFolder(viewsRoot, viewsTarget);
        }

        private void CopyCss(string folder) {
            CopyFolder(IOHelper.MapPath("~/css"), Path.Combine(folder, siteFolder, "css"));
        }

        private void CopyScripts(string folder)
        {
            CopyFolder(IOHelper.MapPath("~/scripts"), Path.Combine(folder, siteFolder, "scripts"));
        }

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
        private FileStream ZipFolder(string folder)
        {
            var filename = $"migration_data_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}.zip";
            var folderInfo = new DirectoryInfo(folder);
            var files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            { 
                foreach(var file in files)
                {
                    var relativePath = file.FullName.Substring(folder.Length + 1);
                    archive.CreateEntryFromFile(file.FullName, relativePath);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            var fs = new FileStream(Path.Combine(_root, filename), FileMode.Create);
            stream.WriteTo(fs);

            return fs;
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

        /// <summary>
        /// Get the folder path for the export
        /// </summary>
        /// <param name="id">The unique id for the folder</param>
        /// <returns>The full path to the folder to export to</returns>
        private string GetExportFolder(string id) => Path.Combine(_root, id);
    }
}
