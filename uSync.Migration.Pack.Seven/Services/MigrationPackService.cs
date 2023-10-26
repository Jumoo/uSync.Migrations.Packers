using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Jumoo.uSync.BackOffice;

using Newtonsoft.Json;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

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

        public Guid CreateExport()
        {
            var id = Guid.NewGuid();
            var folder = Path.Combine(GetExportFolder(id), uSyncFolder);
            CreateExport(folder);
            return id;
        }

        public void GetConfig(Guid id)
        {
            var folder = GetExportFolder(id);
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

        public void CopyViews(Guid id)
        {
            var folder = GetExportFolder(id);
            var viewsRoot = IOHelper.MapPath("~/views");
            var viewsTarget = Path.Combine(folder, siteFolder, "views");
            CopyFolder(viewsRoot, viewsTarget);
        }

        public void CopyFiles(Guid id)
        {
            var folder = GetExportFolder(id);
            CopyCss(folder);
            CopyScripts(folder);
        }

        public MemoryStream ZipExport(Guid id)
        {
            var folder = GetExportFolder(id);
            var stream = ZipFolder(folder);
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
        private MemoryStream ZipFolder(string folder)
        {
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

        private string GetExportFolder(Guid id)
            => Path.Combine(_root, id.ToString());
    }
}
