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

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Sites;
using Unic.SitecoreCMS.Modules.ErrorManager.Extensions;
using Unic.SitecoreCMS.Modules.ErrorManager.Utilities;
using System.Text;

namespace Unic.SitecoreCMS.Modules.ErrorManager.Controls
{
    /// <summary>
    /// Base class for all error pages. First of all we search for an <see cref="Sitecore.Data.Items.Item"/> in the current
    /// <see cref="Sitecore.Sites.SiteContext"/> and in the current <see cref="Sitecore.Globalization.Language"/> (or in the default language
    /// of the current <see cref="Sitecore.Sites.SiteContext"/>). If not found, we take the configured static file as error page. After resolving
    /// the url to the error page, we make a <see cref="System.Net.HttpWebRequest"/> to this page and write the raw content of
    /// the <see cref="System.Net.HttpWebResponse"/> to the current response.
    /// </summary>
    /// <author>Kevin Brechbühl - Unic AG</author>
    public class BaseError : System.Web.UI.Page
    {
        #region Members

        private string _settingsKey = string.Empty;

        /// <summary>
        /// Gets or sets the settings key.
        /// </summary>
        /// <value>
        /// The settings key.
        /// </value>
        protected string SettingsKey
        {
            get
            {
                return _settingsKey;
            }
            set
            {
                string key = Sitecore.Web.WebUtil.GetSafeQueryString("key");
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting(key)))
                {
                    _settingsKey = key;
                }
                else
                {
                    _settingsKey = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        protected int StatusCode
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event and try to resolve the raw data for the error page.
        /// At the end, outputs the raw data of the content we want to display.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // initial parameters
            Language lang = UrlUtil.ResolveLanguage();
            SiteContext site = UrlUtil.ResolveSite(lang);
            string url = string.Empty;

            // Use the static error page if the site or the database is not available
            if (site == null || site.Database == null) {
                url = Sitecore.Web.WebUtil.GetServerUrl() + Sitecore.Configuration.Settings.GetSetting(SettingsKey + ".Static");
            }
            else {
                string availableLanguages = site.Properties["availableLanguages"];

                // general options for generating url
                UrlOptions options = UrlOptions.DefaultOptions;
                options.LanguageEmbedding = LanguageEmbedding.Always;
                options.LanguageLocation = LanguageLocation.QueryString;
                options.Language = lang;
                options.Site = site;
                options.AlwaysIncludeServerUrl = true;

                // get the error item
                string path = Settings.GetBoolSetting("ErrorManager.UseRootPath", false) ? site.RootPath : site.StartPath;
                Item item = site.Database.GetItem(path + Sitecore.Configuration.Settings.GetSetting(SettingsKey + ".Item"));

                // resolve the url for the error page
                if (item != null && item.HasLanguageVersion(lang, availableLanguages))
                {
                    url = LinkManager.GetItemUrl(item, options);
                }
                else
                {
                    Language.TryParse(!string.IsNullOrEmpty(site.Properties["language"]) ? site.Properties["language"] : LanguageManager.DefaultLanguage.Name, out lang);
                    if (item != null && lang != null && item.HasLanguageVersion(lang, availableLanguages))
                    {
                        options.Language = lang;
                        url = LinkManager.GetItemUrl(item, options);
                    }
                    else
                    {
                        url = Sitecore.Web.WebUtil.GetServerUrl() + Sitecore.Configuration.Settings.GetSetting(SettingsKey + ".Static");
                    }
                }

                // append current raw url
                url += url.IndexOf("?") == -1 ? "?" : "&";
                url += "rawUrl=" + Server.UrlEncode(Sitecore.Web.WebUtil.GetRawUrl());
            }

            // parse the page
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            bool ignoreInvalidSSLCertificates = Sitecore.Configuration.Settings.GetBoolSetting("ErrorManager.IgnoreInvalidSSLCertificates", false);

            // add user cookies to the request
            if (Sitecore.Configuration.Settings.GetBoolSetting("ErrorManager.SendClientCookies", false))
            {
                request.CookieContainer = new CookieContainer();
                HttpCookieCollection userCookies = Request.Cookies;
                for (int userCookieCount = 0; userCookieCount < userCookies.Count; userCookieCount++)
                {
                    HttpCookie httpCookie = userCookies.Get(userCookieCount);
                    if (httpCookie.Name != "ASP.NET_SessionId")
                    {
                        Cookie cookie = new Cookie();
                        /*  We have to add the target host because the cookie does not contain the domain information.
                            In this case, this behaviour is not a security issue, because the target is our own platform.
                            Further informations: http://stackoverflow.com/a/460990 
                        */
                        cookie.Domain = request.RequestUri.Host;
                        cookie.Expires = httpCookie.Expires;
                        cookie.Name = httpCookie.Name;
                        cookie.Path = httpCookie.Path;
                        cookie.Secure = httpCookie.Secure;
                        //// Encode cookie value for handling comma separator
                        //// http://stackoverflow.com/questions/1136405/handling-a-comma-inside-a-cookie-value-using-nets-c-system-net-cookie
                        cookie.Value =HttpUtility.UrlEncode(httpCookie.Value);

                        request.CookieContainer.Add(cookie);
                    }
                }
            }

            HttpWebResponse response = null;
            bool hasAddedValidationCallback = false;

            try
            {
                int timeout = 0;
                Int32.TryParse(Sitecore.Configuration.Settings.GetSetting("ErrorManager.Timeout"), out timeout);
                if (timeout == 0)
                {
                    timeout = 60*1000;
                }

                int maxRedirects = 0;
                Int32.TryParse(Sitecore.Configuration.Settings.GetSetting("ErrorManager.MaxRedirects"), out maxRedirects);
                if (maxRedirects == 0)
                {
                    maxRedirects = 3;
                }

                if (ignoreInvalidSSLCertificates)
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                    hasAddedValidationCallback = true;
                }

                // do the request
                request.Timeout = timeout;
                request.MaximumAutomaticRedirections = maxRedirects;
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (WebException ex)
            {
                // we need to catch this, because statuscode of the sitecore default error pages may throwing an exception in the HttpWebResponse object
                response = (HttpWebResponse) ex.Response;
            }
            finally
            {
                // Remove the custom RemoteCertificateValidationCallback due to the global nature of the ServicePointManager
                if (hasAddedValidationCallback)
                {
                    ServicePointManager.ServerCertificateValidationCallback -= new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                }
            }

            // outputs the page
            if (response != null)
            {
                string body = new StreamReader(response.GetResponseStream()).ReadToEnd();

                // Insert image with request to the static page if Analytics is enabled.
                // This is a hotfix for a Sitecore bug, see Sitecore issue #378950
                if (Settings.GetBoolSetting("Analytics.Enabled", false) && site.EnableAnalytics)
                {
                    body = body.Replace("</body>", string.Format("<img src=\"{0}?{1}\" height=\"1\" width=\"1\" border=\"0\"></body>", Sitecore.Configuration.Settings.GetSetting(SettingsKey + ".Static"), base.Request.QueryString));
                }

                Response.Write(body);
            }
            else
            {
                Response.Write("Statuscode: " + StatusCode);
            }
            
            // set statuscode
            if (StatusCode > 0)
            {
                Response.StatusCode = StatusCode;
            }

            // pass through the response we create here
            Response.TrySkipIisCustomErrors = true;
        }

        /// <summary>
        /// Validates the remote certificate without regarding the validity of it.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="policyErrors">The policy errors.</param>
        /// <returns>boolean with the validity result</returns>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
