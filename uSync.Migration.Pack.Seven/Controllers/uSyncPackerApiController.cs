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
        /// <summary>
        ///  finder method (so we can programatically find the route)
        /// </summary>
        public bool GetApi() => true;

        [HttpPost]
        public HttpResponseMessage MakePack()
        {

            var filename = string.Format("migration_data_{0}.zip", DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
            // var filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip";

            var migrationPackService = new MigrationPackService();
            using (var stream = migrationPackService.PackExport())
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
