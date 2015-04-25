using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OpenSourceScrumTool.Startup))]
namespace OpenSourceScrumTool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Configure Auth first so it can be used in SignalR
            ConfigureAuth(app);
            ConfigureSignalR(app);
        }
    }
}
