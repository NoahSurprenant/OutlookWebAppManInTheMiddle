using Microsoft.Extensions.Options;
using Serilog;
using System.Net;

namespace OutlookWebAppManInTheMiddle.Extensions
{
    public static class ServiceExtensions
    {
        public static void RegisterForwardProxyHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient("DefaultReverseProxy", (services, client) =>
            {
            })
            .ConfigurePrimaryHttpMessageHandler((services) =>
            {
                var forwardProxyOptions = services.GetService<IOptionsSnapshot<ForwardProxyOptions>>();
                var isValid = forwardProxyOptions?.Value.IsValid ?? false;

                if (isValid)
                {
                    Log.Logger.Information("Using Proxy {Host}:{Port}", forwardProxyOptions!.Value.Host, forwardProxyOptions.Value.Port);
                    var proxy = new WebProxy(forwardProxyOptions.Value.Host, forwardProxyOptions.Value.PortInt);
                    return new HttpClientHandler()
                    {
                        AllowAutoRedirect = false,
                        UseCookies = false,
                        Proxy = proxy,
                    };
                }
                else
                {
                    Log.Logger.Information("Not using forward proxy");
                    return new HttpClientHandler()
                    {
                        AllowAutoRedirect = false,
                        UseCookies = false,
                    };
                }
            });


        }
    }
}
