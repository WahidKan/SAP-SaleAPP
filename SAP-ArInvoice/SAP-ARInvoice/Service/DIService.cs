using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.NetworkInformation;
using System.Timers;
using Timer = System.Threading.Timer;
using SAP_ARInvoice.Controllers;

namespace SAP_ARInvoice.Service
{
    
    public class DIService : IHostedService, IDisposable
    {
        public HomeController homeController;
        private Timer timer;
        private readonly ILogger<DIService> logger;

        public DIService(ILogger<DIService> logger)
        {
            this.logger = logger;
        }

     

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(o => {
                //action
                //_ = homeController.GetAsync();
                logger.LogInformation($"Background Service");

            },
          null,
          TimeSpan.Zero,
          TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}

