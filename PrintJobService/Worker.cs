using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace PrintJobService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            System.IO.FileSystemWatcher fsw = new FileSystemWatcher();
            fsw.Path = @"c:\public\printdrop";
            fsw.EnableRaisingEvents = true;
            fsw.Created += OnCreated;

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            var path = eventArgs.FullPath;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = path;
            info.Verb = "Print";
            info.UseShellExecute = true;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            Process p = new Process();
            p.StartInfo = info;
            p.Start();
            p.WaitForExit(10000);


            if (false == p.CloseMainWindow())
            {
                p.Kill();
                p.Dispose();
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                _logger.LogError("An exception occured trying to delete the file: {0}.  {1}", path, e.ToString());
            }


        }
    }
}
