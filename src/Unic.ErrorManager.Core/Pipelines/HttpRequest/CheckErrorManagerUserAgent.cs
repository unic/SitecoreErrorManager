using Sitecore.Analytics.Pipelines.ExcludeRobots;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Unic.ErrorManager.Core.Definitions;

namespace Unic.ErrorManager.Core.Pipelines.HttpRequest
{
    public class CheckErrorManagerUserAgent : ExcludeRobotsProcessor
    {
        public override void Process(ExcludeRobotsArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.IsNotNull((object)args.HttpContext, "HttpContext");
            Assert.IsNotNull((object)args.HttpContext.Request, "Request");
            if (args.HttpContext.Request.UserAgent == null || Settings.GetSetting(Constants.RedirectRequestBotSetting) != args.HttpContext.Request.UserAgent)
                return;
            args.IsInExcludeList = true;
        }
    }
}
