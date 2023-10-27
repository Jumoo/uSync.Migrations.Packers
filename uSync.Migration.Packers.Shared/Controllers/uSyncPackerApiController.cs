using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

#if NET452
using Umbraco.Core;
#else 
using Umbraco.Core.Composing;
#endif

using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using uSync.Migration.Packers.Shared.Model;

namespace uSync.Migration.Packers.Shared.Controllers
{
    [PluginController("uSync")]
    public class uSyncPackerApiController : UmbracoAuthorizedApiController
    {
        private readonly MigrationPackService _migrationPackService;

#if NET452
        public uSyncPackerApiController()
        {
            _migrationPackService = new MigrationPackService();
        }
#else
        public uSyncPackerApiController(MigrationPackService migrationPackService)
        {
            _migrationPackService = migrationPackService;
        }
#endif

        [HttpGet]
        public bool GetApi() => true;

        [HttpGet]
        public bool HasContentEdition()
            => TypeFinder.GetTypeByName(MigrationPackers.uSyncContentType) != null;


        [HttpGet]
        public MigrationPackStepResult PackStep(Guid id, int index)
            => _migrationPackService.PerformStep(id, index);

        [HttpPost]
        public HttpResponseMessage GetPack(Guid id)
        {
            using (var stream = _migrationPackService.ZipExport(id))
            {
                if (stream == null)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

                var binaryReader = new BinaryReader(stream);
                var byteLength = new FileInfo(stream.Name).Length;
                var fileName = Path.GetFileName(stream.Name);

                var response = new HttpResponseMessage
                {
                    Content = new ByteArrayContent(binaryReader.ReadBytes((int)byteLength))
                    {
                        Headers =
                        {
                            ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = fileName
                            },
                            ContentType = new MediaTypeHeaderValue("application/x-zip-compressed")
                        }

                    }
                };

                response.Headers.Add("x-filename", fileName);
                return response;
            }
        }
    }
}
