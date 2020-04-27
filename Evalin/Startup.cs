using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Evalin.Startup))]
namespace Evalin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
