using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Bug_Tracker.Startup))]
namespace Bug_Tracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
