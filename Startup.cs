using Microsoft.AspNetCore.Builder;
using System.Globalization;

namespace FileServer2
{
    public class Startup
    {

        //public IConfiguration Configuration { get; }
        public ConfigurationManager configuration { get; }

        public Startup(ConfigurationManager _configuration)
        {
            this.configuration = _configuration;
        }

        public void Configure(IApplicationBuilder _app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
        }

    }

}
