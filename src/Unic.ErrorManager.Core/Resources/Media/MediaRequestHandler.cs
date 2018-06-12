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

using System;

namespace Unic.ErrorManager.Core.Resources.Media
{
    using System.Web;

    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Resources.Media;
    using Sitecore.SecurityModel;

    using Unic.ErrorManager.Core.Extensions;
    using Unic.ErrorManager.Core.Utilities;

    /// <summary>
    ///     Class implements a custom MediaRequestHandler. It redirects to the configured 404 page if a media request is
    ///     invalid.
    ///     <br /><br />
    ///     To add the handler, change the default MediaRequestHandler in web.config from:
    ///     <code>
    /// &lt;system.webServer&gt;
    ///     &lt;handlers&gt;
    ///         &lt;add verb="*" path="sitecore_media.ashx" type="Sitecore.Resources.Media.MediaRequestHandler, Sitecore.Kernel" name="Sitecore.MediaRequestHandler" /&gt;
    ///     &lt;/handlers&gt;
    /// &lt;/system.webServer&gt;
    /// </code>
    ///     to
    ///     <code>
    /// &lt;system.webServer&gt;
    ///     &lt;handlers&gt;
    ///         &lt;add verb="*" path="sitecore_media.ashx" type="Unic.SitecoreCMS.Modules.ErrorManager.Resources.Media.MediaRequestHandler, Unic.SitecoreCMS.Modules.ErrorManager" name="Sitecore.MediaRequestHandler" /&gt;
    ///     &lt;/handlers&gt;
    /// &lt;/system.webServer&gt;
    /// </code>
    ///     and from
    ///     <code>
    /// &lt;system.web&gt;
    ///     &lt;httpHandlers&gt;
    ///         &lt;add verb="*" path="sitecore_media.ashx" type="Sitecore.Resources.Media.MediaRequestHandler, Sitecore.Kernel" /&gt;
    ///     &lt;/httpHandlers&gt;
    /// &lt;/system.web&gt;
    /// </code>
    ///     to
    ///     <code>
    /// &lt;system.web&gt;
    ///     &lt;httpHandlers&gt;
    ///         &lt;add verb="*" path="sitecore_media.ashx" type="Unic.SitecoreCMS.Modules.ErrorManager.Resources.Media.MediaRequestHandler, Unic.SitecoreCMS.Modules.ErrorManager" /&gt;
    ///     &lt;/httpHandlers&gt;
    /// &lt;/system.web&gt;
    /// </code>
    /// </summary>
    /// <author>Kevin Brechbühl - Unic AG</author>
    public class MediaRequestHandler : Sitecore.Resources.Media.MediaRequestHandler
    {
        #region Methods

        /// <summary>
        ///     Process the current request and return false if the media item was not found, so Sitecore
        ///     can go ahead with processing the pipelines. Also check the <c>availableLanguages</c>
        ///     from the current <see cref="Sitecore.Sites.SiteContext" /> to check for a valid language
        ///     version of the media.
        /// </summary>
        /// <param name="context">Current <see cref="System.Web.HttpContext" />.</param>
        /// <returns><c>True</c> if everything was OK, <c>false</c> if the request is invalid or the media was not found.</returns>
        protected override bool DoProcessRequest(HttpContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Assert.IsNotNull(Sitecore.Context.Site, "site");
            MediaRequest mediaRequest = MediaManager.ParseMediaRequest(context.Request);
            if (mediaRequest == null)
            {
                return false;
            }

            bool notfound = false;
            bool noaccess = false;

            string redirect = string.Empty;

            // get media
            Media media = MediaManager.GetMedia(mediaRequest.MediaUri);

            try
            {
                Language lang = UrlUtil.ResolveLanguage();
                // check item in language
                Item mediaItem = Sitecore.Context.Database.GetItem(mediaRequest.MediaUri.MediaPath);
                if (Sitecore.Context.GetSiteName() != "shell"
                    && (mediaItem?.Fields["Blob"] == null
                        || (!mediaItem.Fields["Blob"].Shared && !mediaItem.HasLanguageVersion(lang, Sitecore.Context.Site.Properties["availableLanguages"]))))
                {
                    notfound = true;
                }
            }
            catch (Exception)
            {
                notfound = true;
            }

            // check for media
            if (media == null)
            {
                using (new SecurityDisabler())
                {
                    media = MediaManager.GetMedia(mediaRequest.MediaUri);
                }

                // this would not take an effect
                // mediaItem is read above without security disabler
                // null mediaItem implies true notfound
                // and further notfound has priority over noaccess

                if (media == null)
                {
                    notfound = true;
                }
                else
                {
                    noaccess = true;
                }
            }

            // generate redirect url
            if (notfound)
            {
                redirect = Sitecore.Configuration.Settings.ItemNotFoundUrl;
            }
            else if (noaccess)
            {
                redirect = ((Sitecore.Context.Site.LoginPage != string.Empty)
                    ? Sitecore.Context.Site.LoginPage
                    : Sitecore.Configuration.Settings.NoAccessUrl);
            }

            // redirect if needed
            if (!string.IsNullOrEmpty(redirect))
            {
                if (Sitecore.Configuration.Settings.RequestErrors.UseServerSideRedirect)
                {
                    HttpContext.Current.Items["IsMedia"] = "true";
                    HttpContext.Current.Server.Transfer(redirect);
                }
                else
                {
                    Sitecore.Web.WebUtil.AddQueryString(redirect, "IsMedia", "true");
                    HttpContext.Current.Response.Redirect(redirect);
                }

                return false;
            }

            return this.DoProcessRequest(context, mediaRequest, media);
        }

        #endregion
    }
}