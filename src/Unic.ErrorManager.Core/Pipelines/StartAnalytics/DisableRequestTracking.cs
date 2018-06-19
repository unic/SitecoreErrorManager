namespace Unic.ErrorManager.Core.Pipelines.StartAnalytics
{
    using System;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;
    using Sitecore.Web;
    using Definitions;

    /// <summary>
    /// Disables the xDB tracker for requests from the error manager to the error pages due to session locks
    /// </summary>
    public class DisableRequestTracking
    {
        public virtual void Process(PipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            if (!this.ShouldExecute()) return;

            if (this.ShouldDisableTracking())
            {
                args.AbortPipeline();
            }
        }

        protected virtual bool ShouldExecute()
        {
            return Settings.GetBoolSetting(Constants.DisableTrackingSetting, true);
        }

        protected virtual bool ShouldDisableTracking()
        {
            var disableTrackingParameterValueQuery = WebUtil.GetQueryString(Constants.DisableTrackingParameterName);
            if (string.IsNullOrWhiteSpace(disableTrackingParameterValueQuery)) return false;

            var disableTrackingParameterValue = Settings.GetSetting(Constants.DisableTrackingParameterValueSetting);
            return string.Equals(disableTrackingParameterValueQuery, disableTrackingParameterValue, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
