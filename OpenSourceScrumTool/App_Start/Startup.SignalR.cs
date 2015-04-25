using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Owin;

namespace OpenSourceScrumTool
{
    public partial class Startup
    {
        public void ConfigureSignalR(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
#if DEBUG
            app.MapSignalR(new HubConfiguration()
            {
                EnableJavaScriptProxies = true,
                EnableDetailedErrors = true
            });
#else
            app.MapSignalR();
#endif
        }
    }
}