namespace Unic.ErrorManager.Core.Pipelines.HttpRequest
{
    using System;
    using System.Web;
    using Sitecore;
    using Sitecore.Analytics.Pipelines.ExcludeRobots;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Constants = Definitions.Constants;

    [UsedImplicitly]
    public class CheckErrorManagerUserAgent : ExcludeRobotsProcessor
    {
        private HttpContextBase context;

        public HttpContextBase HttpContext
        {
            get
            {
                if (this.context != null)
                {
                    return this.context;
                }

                if (System.Web.HttpContext.Current == null)
                {
                    return null;
                }

                return this.context = new HttpContextWrapper(System.Web.HttpContext.Current);
            }
        }

        public override void Process(ExcludeRobotsArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            var httpContext = this.HttpContext;
            var userAgent = httpContext.Request.UserAgent;
            var userAgentSetting = Settings.GetSetting(Constants.ErrorManagerUserAgentSetting);

            if (userAgent == null || !string.Equals(userAgentSetting, userAgent, StringComparison.InvariantCultureIgnoreCase)) return;

            args.IsInExcludeList = true;
        }
    }
}