using Hangfire;
using Hangfire.SqlServer;
using System;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Dsp.Web.Extensions
{
    public class ApplicationPreload : System.Web.Hosting.IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            HangfireBootstrapper.Instance.Start();
        }
    }

    public class HangfireBootstrapper : IRegisteredObject
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object _lockObject = new object();
        private bool _started;

        private BackgroundJobServer _backgroundJobServer;

        private HangfireBootstrapper()
        {
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);

                var options = new SqlServerStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(60)
                };
                var dbConnectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(dbConnectionString, options);

                _backgroundJobServer = new BackgroundJobServer();
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_backgroundJobServer != null)
                {
                    _backgroundJobServer.Dispose();
                }

                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }
}