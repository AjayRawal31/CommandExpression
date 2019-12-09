using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TukTuk.Startup))]
namespace TukTuk
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
