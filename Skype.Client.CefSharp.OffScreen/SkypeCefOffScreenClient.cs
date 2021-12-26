using System;
using System.IO;
using CefSharp;
using CefSharp.OffScreen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Skype.Client.CefSharp.OffScreen
{
    public class SkypeCefOffScreenClient : SkypeCefClient, IDisposable
    {
        static SkypeCefOffScreenClient()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkypeAutomation");
            System.IO.Directory.CreateDirectory(path);

            CefSettings setngs = new CefSettings();
            setngs.LogSeverity = LogSeverity.Error;
            //setngs.CefCommandLineArgs.Remove("enable-media-stream");
            //setngs.CachePath = path;

            //Perform dependency check to make sure all relevant resources are in our output directory.
            //Cef.Initialize(new CefSettings() { LogSeverity = LogSeverity.Error }, performDependencyCheck: true, browserProcessHandler: null);
            Cef.Initialize(setngs, performDependencyCheck: true, browserProcessHandler: null);
        }

        public SkypeCefOffScreenClient() : this(NullLoggerFactory.Instance)
        {
        }

        public SkypeCefOffScreenClient(ILoggerFactory loggerFactory) : base(new ChromiumWebBrowser(), loggerFactory)
        {
        }

        public void Dispose()
        {
            Cef.Shutdown();
        }
    }
}
