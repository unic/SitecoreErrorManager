using System;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.Web;
using Unic.ErrorManager.Core.Definitions;

namespace Unic.ErrorManager.Core.Pipelines.StartAnalytics
{
    /// <summary>
    /// Disables the xDB tracker for requests from the error manager to the error pages due to session locks
    /// </summary>
    public class DisableRequestTracking
    {
        public virtual void Process(PipelineArgs args)
        {
            var disableTrackingValue = string.Empty;

            if (Convert.ToBoolean(Settings.GetSetting(Constants.EnableDisableTrackingSetting)))
            {
                disableTrackingValue = WebUtil.GetQueryString(Constants.DisableTrackingParameterName);
            }

            if (!string.IsNullOrEmpty(disableTrackingValue) 
                && disableTrackingValue.Equals(Settings.GetSetting(Constants.DisableTrackingParameterValueSetting, string.Empty)))
            {
                args.AbortPipeline();
            }
        }
    }
}
