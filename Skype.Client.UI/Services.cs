using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skype.Client.CefSharp.OffScreen;
using Skype.Client.UI.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI
{
    public static  class Service
    {
        private static IServiceCollection _services;
        public static ServiceProvider Provider { get; private set; }
        static Service()
        {
            _services = new ServiceCollection();
            _services.AddLogging(logging =>
            {
                logging.AddDebug();
                logging.AddUILogger();
            });
            _services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);

            _services.AddSingleton<SkypeCefOffScreenClient>();

            Provider = _services.BuildServiceProvider();
        }
    }
}
