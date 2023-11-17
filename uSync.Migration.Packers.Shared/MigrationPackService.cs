using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Newtonsoft.Json;

using Umbraco.Core;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;
using uSync.Migration.Packers.Shared.Model;

#if NET452
using Umbraco.Core.Configuration;
using Jumoo.uSync.BackOffice;
#else
using Umbraco.Core.Composing;
using uSync8.BackOffice;
using uSync8.BackOffice.SyncHandlers;
#endif

namespace uSync.Migration.Packers.Shared
{
    public class MigrationPackService 
    {
        private const string _siteFolder = MigrationPackers.SiteFolder;
        private const string _uSyncFolder = MigrationPackers.uSyncFolder;
        private readonly string _root;

        private readonly IGridConfig _gridConfig;

#if NET472_OR_GREATER
        private readonly uSyncService _uSyncService;
#endif

#if NET452
        public MigrationPackService()
#else
        public MigrationPackService(uSyncService uSyncService)
#endif
        {
            _root = IOHelper.MapPath("~/uSync/MigrationPacks");

#if NET452
            var appPlugins = "..\\App_Plugins";
            var configFolder = "..\\ConfigFolder";
            var debugging = false;

            _gridConfig = UmbracoConfig.For.GridConfig(
                ApplicationContext.Current.ProfilingLogger.Logger,
                ApplicationContext.Current.ApplicationCache.RuntimeCache,
                new DirectoryInfo(appPlugins),
                new DirectoryInfo(configFolder),
                debugging);

#else 

            _uSyncService = uSyncService;
            _gridConfig = Current.Configs.Grids();

#endif
        }

        public Guid CreateExport(Guid id)
        {
            if (id == Guid.Empty) id = Guid.NewGuid();
            var folder = Path.Combine(GetExportFolder(id), _uSyncFolder);

            CreateExport(folder);

            // we don't hash the top level folders.
            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                HashFolders(subFolder);
            }

            return id;
        }

        private void CreateExport(string folder)
        {
#if NET452
            uSyncBackOfficeContext.Instance.ExportAll(folder);
#else
            var options = new SyncHandlerOptions("default", HandlerActions.Export);
            _ = _uSyncService.Export(folder, options, null);
#endif
        }

        private void HashFolders(string folder)
        {
            foreach (var file in Directory.GetFiles(folder, "*.config").ToList())
            {
                var newname = Path.Combine(Path.GetDirectoryName(file), $"{file.GenerateHash()}.config");
                File.Move(file, newname);
            }

            foreach (var directory in Directory.GetDirectories(folder).ToList().Select((path, index) => (path, index)))
            {
                HashFolders(directory.path);

                var newName = Path.Combine(Path.GetDirectoryName(directory.path), $"{directory.index:0000}");
                Directory.Move(directory.path, newName);
            }

        }

        public Guid GetConfig(Guid id)
        {
            var folder = GetExportFolder(id);

            var configJson = JsonConvert.SerializeObject(_gridConfig
                .EditorsConfig.Editors, Formatting.Indented);

            var configFile = Path.Combine(folder, _siteFolder, "config", "grid.editors.config.js");

            Directory.CreateDirectory(Path.GetDirectoryName(configFile));
            File.WriteAllText(configFile, configJson);
            return id;
        }

        public Guid CopyViews(Guid id)
        {
            CopySiteFolder("views", id);
            return id;
        }

        public Guid CopyFiles(Guid id)
        {
            CopySiteFolder("css", id);
            CopySiteFolder("scripts", id);
            return id;
        }

        private void CopySiteFolder(string folderName, Guid id)
        {
            var targetFolder = GetExportFolder(id);
            CopyFolder(IOHelper.MapPath($"~/{folderName}"), Path.Combine(targetFolder, _siteFolder, folderName));
        }

        private void CopyFolder(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(sourceFolder)) return;

            var files = Directory.GetFiles(sourceFolder, "*.*");
            Directory.CreateDirectory(targetFolder);
            foreach (var file in files)
            {
                var target = Path.Combine(targetFolder, Path.GetFileName(file));
                File.Copy(file, target);
            }

            foreach (var folder in Directory.GetDirectories(targetFolder))
            {
                CopyFolder(folder, Path.Combine(targetFolder, Path.GetFileName(folder)));
            }
        }

        private void CleanFolder(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // if the directory is locked it won't delete
                // that's not ideal, but it doesn't effect the 
                // export process here. 
            }
        }

        private string GetExportFolder(Guid id)
            => Path.Combine(_root, id.ToString());


        public FileStream ZipExport(Guid id)
        {
            var folder = GetExportFolder(id);
            var stream = ZipFolder(folder);
            CleanFolder(folder);
            return stream;
        }

        private FileStream ZipFolder(string folder)
        {
            var filename = $"migration_data_{DateTime.Now:yyyy_MM_dd_HHmmss}.zip";
            var folderInfo = new DirectoryInfo(folder);
            var files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            var stream = new MemoryStream();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, System.Text.Encoding.UTF8))
            {
                foreach (var file in files)
                {
                    var relativePath = file.FullName.Substring(folder.Length + 1)
                        .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    archive.CreateEntryFromFile(file.FullName, relativePath);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            var fileStream = new FileStream(Path.Combine(_root, filename), FileMode.Create);
            stream.WriteTo(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            return fileStream;

        }

        /// <summary>
        ///  Perform a step. 
        /// </summary>
        /// <remarks>
        ///  this is the entry point and the main pump for the packer 
        ///  process. 
        ///  
        ///  calling this, repeatedly until you get a 'completed'
        ///  will mean you have a zip file ready to be downloaded. 
        /// </remarks>
        public MigrationPackStepResult PerformStep(Guid id, int stepIndex)
        {
            if (id == Guid.Empty) id = Guid.NewGuid();

            var steps = Steps();
            if (stepIndex >= steps.Length)
                return new MigrationPackStepResult(id, true);

            var step = steps[stepIndex];
            var resultId  = step.Action(id);

            return new MigrationPackStepResult(resultId,
                stepIndex + 1 >= steps.Length);
        }


        public MigrationPackStep[] Steps()
        {
            return new MigrationPackStep[]
            {
                new MigrationPackStep("Export", CreateExport),
                new MigrationPackStep("Views", CopyViews),
                new MigrationPackStep("Config", GetConfig),
                new MigrationPackStep("Files", CopyFiles)
            };
        }
    }
}
