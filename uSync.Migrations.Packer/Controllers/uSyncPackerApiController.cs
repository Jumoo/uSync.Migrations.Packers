using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using uSync.Migrations.Packer.Services;

namespace uSync.Migrations.Packer.Controllers
{

    [PluginController("uSync")]
    public class uSyncPackerApiController : UmbracoAuthorizedApiController
    {
        private readonly MigrationPackService _migrationPackService;

        public uSyncPackerApiController(MigrationPackService migrationPackService)
        {
            _migrationPackService = migrationPackService;
        }

        public bool GetApi() => true;

        [HttpGet]
        public bool HasContentEdition()
            => TypeFinder.GetTypeByName("uSync8.ContentEdition.uSyncContent") != null;

        [HttpPost]
        public HttpResponseMessage MakePack()
        {
            var filename = string.Format("v8_migration_{0}.zip", DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
            // var filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip";

            using (var stream = _migrationPackService.PackExport())
            {
                if (stream != null)
                {
                    var response = new HttpResponseMessage
                    {
                        Content = new ByteArrayContent(stream.ToArray())
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

            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        }
    }
}
