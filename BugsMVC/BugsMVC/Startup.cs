using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BugsMVC.Startup))]
namespace BugsMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
