using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QRSPortal2.Startup))]
namespace QRSPortal2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
