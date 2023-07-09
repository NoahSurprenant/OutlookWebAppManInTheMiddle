using HttpContextMapper;
using System.Collections.Specialized;

namespace OutlookWebAppManInTheMiddle
{
    public class OutlookWebAppContextMapper : ContextMapper
    {
        public OutlookWebAppContextMapper(IConfiguration config, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) : base(httpClientFactory, loggerFactory)
        {
            var target = config.GetValue<string>("TargetUrlWithProtocol");
            if (target is null) throw new ArgumentNullException(nameof(target), "TargetUrlWithProtocol must be provided in configuration");

            TargetUrlWithProtocol = target;
            // TODO: fix how we are setting the builder
            UriBuilder = new UriBuilder(TargetUrlWithProtocol);

            DisableSetCookieEncoding = true;
        }

        protected override Task SetUriBuilderPathAndQuery(PathString path, NameValueCollection query)
        {
            if (HttpContext.Request.Query.ContainsKey("url"))
            {
                var urlParameter = query["url"];
                query["url"] = ReplaceProxyWithTarget(ref urlParameter);
            }
            return base.SetUriBuilderPathAndQuery(path, query);
        }

        protected override Task FormRequestContent(Dictionary<string, string> formDictionary)
        {
            if (formDictionary.ContainsKey("destination"))
            {
                var destinationValue = formDictionary["destination"];
                destinationValue = ReplaceProxyWithTarget(ref destinationValue);
                formDictionary["destination"] = destinationValue;
            }
            return Task.CompletedTask;
        }
    }
}
