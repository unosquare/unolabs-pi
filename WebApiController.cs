using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs;
using Unosquare.Labs.WiringPi;
using Constants = Unosquare.Labs.WiringPi.Constants;

namespace unopi
{
    internal class ApiController : WebApiController
    {
        private readonly string photodir = Path.Combine(Directory.GetCurrentDirectory(), "ui");

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        [WebApiHandler(HttpVerbs.Get, "/api/takephoto")]
        public bool TakePhoto(WebServer server, HttpListenerContext context)
        {
            try
            {
                if (IsRunningOnMono())
                {
                    var process = Process.Start("raspistill",
                        "-o /home/pi/target/ui/" + DateTime.Now.ToString("yyyMMdd-HHmm") + ".jpg");

                    process.WaitForExit();
                }
                else
                {
                    var filename = Path.Combine(photodir, DateTime.Now.ToString("yyyMMdd-HHmm") + ".jpg");

                    (new WebClient()).DownloadFile("http://unosquare.github.io/assets/embedio.png", filename);
                }

                var dir = new DirectoryInfo(photodir);
                var files = dir.GetFiles("*.jpg").Select(x => x.Name).Take(10);

                return context.JsonResponse(files);
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Get, "/api/photos")]
        public bool GetPhotos(WebServer server, HttpListenerContext context)
        {
            try
            {
                var dir = new DirectoryInfo(photodir);
                var files = dir.GetFiles("*.jpg").Select(x => x.Name).Take(10);

                return context.JsonResponse(files);
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Get, "/api/io/*")]
        public async Task<bool> Io(WebServer server, HttpListenerContext context)
        {
            var lastSegment = context.Request.Url.Segments.Last();

            var bcmPinNumber = int.Parse(lastSegment);

            CoreFunctions.SetPinMode(bcmPinNumber, Constants.PinMode.Output);
            CoreFunctions.WriteBit(bcmPinNumber, true);
            await Task.Delay(500);
            CoreFunctions.WriteBit(bcmPinNumber, false);

            return true;
        }

        public static Dictionary<int, int> ButtonCheck = new Dictionary<int, int>();

        [WebApiHandler(HttpVerbs.Get, "/api/buttonio/*")]
        public async Task<bool> ButtonIo(WebServer server, HttpListenerContext context)
        {
            var lastSegment = context.Request.Url.Segments.Last();
            var bcmPinNumber = int.Parse(lastSegment);

            return context.JsonResponse(new
            {
                count = ButtonCheck[bcmPinNumber]
            });
        }

        protected bool HandleError(HttpListenerContext context, Exception ex, int statusCode = 500)
        {
            var errorResponse = new
            {
                Title = "Unexpected Error",
                ErrorCode = ex.GetType().Name,
                Description = ex.ExceptionMessage(),
            };

            context.Response.StatusCode = statusCode;
            return context.JsonResponse(errorResponse);
        }
    }
}