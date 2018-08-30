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
        public override void Process(ExcludeRobotsArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            if (!this.ShouldExecute()) return;

            var context = this.GetHttpContext();
            var request = context?.Request;
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

        protected virtual HttpContextWrapper GetHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}