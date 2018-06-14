using System;
using System.Web;
using Sitecore.Analytics.Pipelines.ExcludeRobots;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Unic.ErrorManager.Core.Definitions;

namespace Unic.ErrorManager.Core.Pipelines.HttpRequest
{
    public class CheckErrorManagerUserAgent : ExcludeRobotsProcessor
    {
        public HttpContextBase HttpContext
        {
            get
            {
                if (System.Web.HttpContext.Current != null)
                {
                    return new HttpContextWrapper(System.Web.HttpContext.Current);
                }

                return null;
            }
        }

        public override void Process(ExcludeRobotsArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            var httpContext = this.HttpContext;
            var userAgent = httpContext.Request.UserAgent;

            if (userAgent == null || !string.Equals(Settings.GetSetting(Constants.ErrorManagerUserAgentSetting), userAgent, StringComparison.InvariantCultureIgnoreCase)) return;
            
            args.IsInExcludeList = true;
        }
    }
}