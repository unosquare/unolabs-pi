using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Log;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.WiringPi;
using Constants = Unosquare.Labs.WiringPi.Constants;

namespace unopi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            #region Setup buttons

            Setup.WiringPiSetupGpio();
            CoreFunctions.SetPinMode(26, Constants.PinMode.Input);

            Task.Run(() =>
            {
                while (true)
                {
                    var x = CoreFunctions.ReadBit(26) ? 0 : 1;

                    if (ApiController.ButtonCheck.ContainsKey(26))
                        ApiController.ButtonCheck[26] += x;
                    else
                        ApiController.ButtonCheck.Add(26, x);

                    Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            #endregion

            var url = "http://*:9696/";
            if (args.Length > 0)
                url = args[0];

            // Our web server is disposable. Note that if you don't want to use logging,
            // there are alternate constructors that allow you to skip specifying an ILog object.
            using (var server = new WebServer(url, new SimpleConsoleLog()))
            {
                server.WithWebApiController<ApiController>();
                server.RegisterModule(new CorsModule());

                // Here we setup serving of static files
                server.RegisterModule(new StaticFilesModule(Directory.GetCurrentDirectory()));

                // The static files module will cache small files in ram until it detects they have been modified.
                server.Module<StaticFilesModule>().UseRamCache = true;
                server.Module<StaticFilesModule>().DefaultExtension = ".html";
                // We don't need to add the line below. The default document is always index.html.
                //server.Module<Modules.StaticFilesWebModule>().DefaultDocument = "index.html";

                // Once we've registered our modules and configured them, we call the RunAsync() method.
                // This is a non-blocking method (it return immediately) so in this case we avoid
                // disposing of the object until a key is pressed.
                //server.Run();
                server.RunAsync();
                
                // Wait for any key to be pressed before disposing of our web server.
                // In a service we'd manage the lifecycle of of our web server using
                // something like a BackgroundWorker or a ManualResetEvent.
                Console.ReadKey(true);
            }
        }
    }
}