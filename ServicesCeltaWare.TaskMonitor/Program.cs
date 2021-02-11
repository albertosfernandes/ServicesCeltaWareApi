using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace ServicesCeltaWare.TaskMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build().Run();
            //var isService = !(Debugger.IsAttached || args.Contains("--console"));

            //if (isService)
            //{
            //    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            //    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            //    Directory.SetCurrentDirectory(pathToContentRoot);
            //}

            //// CreateHostBuilder(args).Build().Run();
            //var builder = CreateWebHostBuilder(
            //args.Where(arg => arg != "--console").ToArray());

            //var host = builder.Build();

            //if (isService)
            //{
            //    // To run the app without the CustomWebHostService change the
            //    // next line to host.RunAsService();
            //    host.Run();
            //}
            //else
            //{
            //    host.Run();
            //}
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //    .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<Worker>();
        //        });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });

    }
}
