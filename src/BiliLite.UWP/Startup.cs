using BiliLite.Extensions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiliLite
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMapper();
            services.AddViewModels();

            services.AddSingleton<CookieService>();
            
            services.AddSingleton<GrpcService>();
        }
    }
}
