using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(com.parkkl.backend.Startup))]

namespace com.parkkl.backend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}