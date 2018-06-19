#region Copyright (C) Unic AG

// Copyright (C) Unic AG
// http://www.unic.com
// 
// This module is free software: you can redistribute it and/or modify
// it under the terms of the  GNU Lesser General Public License 
// Version 3.0 as published by the Free Software Foundation.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this module. If not, see http://opensource.org/licenses/lgpl-3.0.

#endregion

namespace Unic.ErrorManager.Core.Pipelines.HttpRequest
{
    using Sitecore;
    using Sitecore.Pipelines.HttpRequest;
    using Extensions;

    /// <summary>
    /// Pipeline processor to resolve if the resolved context item has a valid language version. You can speficy available (valid) languages
    /// with a comma-separated list of language names in the &lt;site&gt;-configuration in the attribute <c>availableLanguages</c>. This
    /// attribute is optional, if no attribute is configure, it only checks if the context item has a language version in the resolved context language.
    /// <br /><br />
    /// The pipeline has to be configured in the web.config as a &lt;httpRequestBegin&gt;-pipeline directly after <see cref="Sitecore.Pipelines.HttpRequest.ItemResolver"/>.
    /// Please see <c>Unic.Modules.ErrorManager.config</c> for a configuration example.
    /// </summary>
    /// <author>Kevin Brechbühl - Unic AG</author>
    [UsedImplicitly]
    public class ItemResolver : HttpRequestProcessor
    {
        /// <summary>
        /// Checks if the resolved context item has a valid language version. If no, set the context item to null, so
        /// the 404 handling should show the not found page. Pipeline only do anything if we are not in the Page Editor.
        /// </summary>
        /// <param name="args">Http request arguments.</param>
        public override void Process(HttpRequestArgs args)
        {
            if (Sitecore.Context.Item == null || Sitecore.Context.Database == null)
            {
                return;
            }

            // check for requets on /sitecore/content and show 404 if request contains the string
            if (Sitecore.Context.PageMode.IsNormal && Sitecore.Context.GetSiteName() != "shell" && Sitecore.Web.WebUtil.GetRawUrl().IndexOf("/sitecore/content") > -1)
            {
                Sitecore.Context.Item = null;
                return;
            }

            // Set language not found page if language version is not available
            if (Sitecore.Context.Item != null &&
                !Sitecore.Context.PageMode.IsExperienceEditor &&
                Sitecore.Context.Site.Name != "shell" &&
                (System.Web.HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"] != "/default.aspx" ||
                Sitecore.Web.WebUtil.GetRawUrl() == "/" + Sitecore.Context.Language))
            {
                if (!Sitecore.Context.Item.HasLanguageVersion(Sitecore.Context.Language, Sitecore.Context.Site.Properties["availableLanguages"]))
                {
                    Sitecore.Context.Item = null;
                }
            }
        }
    }
}
