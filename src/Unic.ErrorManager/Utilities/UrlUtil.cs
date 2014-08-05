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
// 
// #endregion

#endregion

namespace Unic.ErrorManager.Utilities
{
    using Sitecore.Globalization;
    using Sitecore.Sites;

    /// <summary>
    ///     Utility class with methods to resolve parameters and values out of the url.
    /// </summary>
    public class UrlUtil
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Resolves the <see cref="Sitecore.Globalization.Language" /> based on the current rawurl. There are several steps to
        ///     check for a valid language:
        ///     <list type="number">
        ///         <item>Check for querystring parameter <c>sc_lang</c></item>
        ///         <item>Check for querystring parameter <c>la</c> (for media requests)</item>
        ///         <item>Check for language in the path</item>
        ///     </list>
        /// </summary>
        /// <returns>Language if it could be resolved from the rawUrl, the context language otherwise.</returns>
        /// <author>Kevin Brechbühl - Unic AG</author>
        public static Language ResolveLanguage()
        {
            Language langContext = null;
            Language.TryParse(
                Sitecore.Web.WebUtil.ExtractUrlParm("sc_lang", Sitecore.Web.WebUtil.GetRawUrl()),
                out langContext);
            if (langContext != null && !string.IsNullOrEmpty(langContext.Name))
            {
                return langContext;
            }

            Language.TryParse(
                Sitecore.Web.WebUtil.ExtractUrlParm("la", Sitecore.Web.WebUtil.GetRawUrl()),
                out langContext);
            if (langContext != null && !string.IsNullOrEmpty(langContext.Name))
            {
                return langContext;
            }

            Language.TryParse(
                Sitecore.Web.WebUtil.ExtractLanguageName(Sitecore.Web.WebUtil.GetRawUrl()),
                out langContext);
            if (langContext != null && !string.IsNullOrEmpty(langContext.Name))
            {
                return langContext;
            }

            return Sitecore.Context.Language;
        }

        /// <summary>
        ///     Resolves the <see cref="Sitecore.Sites.SiteContext" /> based on the current rawurl. There are several steps to
        ///     check for a valid site:
        ///     <list type="number">
        ///         <item>Check for querystring parameter <c>sc_site</c></item>
        ///         <item>Check for querystring parameter <c>site</c></item>
        ///         <item>Check for site in the path</item>
        ///     </list>
        /// </summary>
        /// <param name="lang">The current language.</param>
        /// <returns>Site if it could be resolved from the rawUrl, the context site otherwise.</returns>
        public static SiteContext ResolveSite(Language lang)
        {
            SiteContext siteContext = null;

            string querystring = Sitecore.Web.WebUtil.ExtractUrlParm("sc_site", Sitecore.Web.WebUtil.GetRawUrl());
            if (!string.IsNullOrEmpty(querystring))
            {
                siteContext = SiteContextFactory.GetSiteContext(querystring);
                if (siteContext != null)
                {
                    return siteContext;
                }
            }

            querystring = Sitecore.Web.WebUtil.ExtractUrlParm("site", Sitecore.Web.WebUtil.GetRawUrl());
            if (!string.IsNullOrEmpty(querystring))
            {
                siteContext = SiteContextFactory.GetSiteContext(querystring);
                if (siteContext != null)
                {
                    return siteContext;
                }
            }

            siteContext = SiteContextFactory.GetSiteContext(
                Sitecore.Web.WebUtil.GetHostName(),
                Sitecore.Web.WebUtil.GetRawUrl().Replace("/" + lang + "/", "/"));
            if (siteContext != null)
            {
                return siteContext;
            }

            return Sitecore.Context.Site;
        }

        #endregion
    }
}