using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.Loggers
{
    public static class UILoggerExtensions
    {
        public static ILoggingBuilder AddUILogger(this ILoggingBuilder factory)
        {
            factory.AddProvider(new UILoggerProvider());
            return factory;
        }
    }
}
