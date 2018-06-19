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

            if (!this.ShouldExecute()) return;

            var request = this.HttpContext?.Request;
            args.IsInExcludeList = this.CheckUserAgent(request);
        }

        protected virtual bool ShouldExecute()
        {
            return Settings.GetBoolSetting(Constants.EnableAgentHeaderCheckSetting, true);
        }

        protected virtual bool CheckUserAgent(HttpRequestBase request)
        {
            var userAgent = request?.UserAgent;
            if (userAgent == null) return false;

            var userAgentSetting = Settings.GetSetting(Constants.UserAgentSetting);
            return string.Equals(userAgent, userAgentSetting, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}