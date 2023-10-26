using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using uSync.Migration.Pack.Seven.Services;

namespace uSync.Migration.Pack.Seven.Controllers
{
    [PluginController("uSync")]
    public class uSyncPackerApiController : UmbracoAuthorizedApiController
    {
        MigrationPackService _packService;

        public uSyncPackerApiController()
        { 
            _packService = new MigrationPackService();
        }

        /// <summary>
        ///  finder method (so we can programatically find the route)
        /// </summary>
        public bool GetApi() => true;

        [HttpGet]
        public Guid CreateExport() => _packService.CreateExport();

        [HttpGet]
        public void GetConfig(Guid id) => _packService.GetConfig(id);

        [HttpGet]
        public void CopyViews(Guid id) => _packService.CopyViews(id);

        [HttpGet]
        public void CopyFiles(Guid id) => _packService.CopyFiles(id);


        [HttpPost]
        public HttpResponseMessage ZipFolder(Guid id)
        {
            var migrationPackService = new MigrationPackService();
            using (var stream = migrationPackService.ZipExport(id))
            {
                // If there is no stream we can exit early, something went wrong :(
                if (stream == null) return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
                
                System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(stream);
                long byteLength = new System.IO.FileInfo(stream.Name).Length;
                var filename = Path.GetFileName(stream.Name);
                    
                var response = new HttpResponseMessage
                {
                    Content = new ByteArrayContent(binaryReader.ReadBytes((int)byteLength))
                    {
                        Headers =
                        {
                            ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = filename
                            },
                            ContentType = new MediaTypeHeaderValue("application/x-zip-compressed")
                        }
                    }
                };

                response.Headers.Add("x-filename", filename);
                return response;
            }
        }
    }
}
