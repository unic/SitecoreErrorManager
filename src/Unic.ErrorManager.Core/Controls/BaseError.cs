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
namespace Unic.ErrorManager.Core.Controls
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using Sitecore.Analytics;
    using Sitecore.Configuration;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Exceptions;
    using Sitecore.Globalization;
    using Sitecore.Links;
    using Sitecore.Sites;
    using Sitecore.Web;

    using Unic.ErrorManager.Core.Extensions;
    using Unic.ErrorManager.Core.Utilities;

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
            get { return this._settingsKey; }
            set
            {
                string key = Sitecore.Web.WebUtil.GetSafeQueryString("key");
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting(key)))
                {
                    this._settingsKey = key;
                }
                else
                {
                    this._settingsKey = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        protected int StatusCode { get; set; }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event and try to resolve the raw data for the error page.
        /// At the end, outputs the raw data of the content we want to display.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // add support to all versions of tls
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            base.OnLoad(e);
            var isUsingStatic = Sitecore.Configuration.Settings.GetBoolSetting(SettingsKey + ".UseStatic", false);
            // initial parameters
            Language lang = UrlUtil.ResolveLanguage();
            SiteContext site = UrlUtil.ResolveSite(lang);
            StringBuilder url = null;

            // Use the static error page if the site or the database is not available
            if (isUsingStatic || site?.Database == null)
            {
                url = new StringBuilder(WebUtil.GetServerUrl() + Settings.GetSetting(this.SettingsKey + ".Static"));
            }
            else
            {
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
                Item item = site.Database.GetItem(path + Settings.GetSetting(this.SettingsKey + ".Item"));

                // resolve the url for the error page
                if (item?.HasLanguageVersion(lang, availableLanguages) == true)
                {
                    url = new StringBuilder(LinkManager.GetItemUrl(item, options));
                }
                else
                {
                    Language.TryParse(!string.IsNullOrEmpty(site.Properties["language"]) ? site.Properties["language"] : LanguageManager.DefaultLanguage.Name, out lang);
                    if (item != null && lang != null && item.HasLanguageVersion(lang, availableLanguages))
                    {
                        options.Language = lang;
                        url = new StringBuilder(LinkManager.GetItemUrl(item, options));
                    }
                    else
                    {
                        url = new StringBuilder(WebUtil.GetServerUrl() + Settings.GetSetting(this.SettingsKey + ".Static"));
                    }
                }

                // append current raw url
                url.Append(url.ToString().IndexOf("?") == -1 ? "?" : "&");
                url.Append("rawUrl=" + this.Server.UrlEncode(WebUtil.GetRawUrl()));

                // add the tracking disable parameter
                url.Append(string.Format("&{0}={1}", Definitions.Constants.DisableTrackingParameterName, Settings.GetSetting(Definitions.Constants.DisableTrackingParameterValueSetting, string.Empty)));

                // change display mode to normal
                url.Append(string.Format("&{0}={1}", Definitions.Constants.DisplayModeParameterName, Definitions.Constants.DisplayModeParameterValueSetting));
            }

            // if path to the target error page is same as requested path, probably we are in the infinite loop, use LayoutNotFoundUrl.Static if defined  
            if (new Uri(url.ToString()).AbsolutePath == new Uri(WebUtil.GetServerUrl() + WebUtil.GetRawUrl()).AbsolutePath)
            {
                var layoutNotFoundUrlStatic = Settings.GetSetting("LayoutNotFoundUrl.Static");

                if (!string.IsNullOrWhiteSpace(layoutNotFoundUrlStatic))
                {
                    url = new StringBuilder(Uri.IsWellFormedUriString(layoutNotFoundUrlStatic, UriKind.Absolute) ? layoutNotFoundUrlStatic : WebUtil.GetServerUrl() + layoutNotFoundUrlStatic);
                }
            }

            // parse the page
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url.ToString());

            // add user cookies to the request
            if (Settings.GetBoolSetting("ErrorManager.SendClientCookies", false))
            {
                this.AddRequestCookies(request);
            }

            // handle header forwarding
            this.HandleHeaderForwarding(request);

            // basic authentication handling
            this.HandleBasicAuthentication(request);
            
            HttpWebResponse response = null;
            bool hasAddedValidationCallback = false;

            try
            {
                var timeout = Settings.GetIntSetting("ErrorManager.Timeout", 0);
                if (timeout == 0)
                {
                    timeout = 60 * 1000;
                }

                var maxRedirects = Settings.GetIntSetting("ErrorManager.MaxRedirects", 0);
                if (maxRedirects == 0)
                {
                    maxRedirects = 3;
                }

                if (Settings.GetBoolSetting("ErrorManager.IgnoreInvalidSSLCertificates", false))
                {
                    ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                    hasAddedValidationCallback = true;
                }

                // do the request
                request.Timeout = timeout;
                request.MaximumAutomaticRedirections = maxRedirects;
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                // we need to catch this, because statuscode of the sitecore default error pages may throwing an exception in the HttpWebResponse object
                response = (HttpWebResponse)ex.Response;
            }
            finally
            {
                // Remove the custom RemoteCertificateValidationCallback due to the global nature of the ServicePointManager
                if (hasAddedValidationCallback)
                {
                    ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                }
            }

            // outputs the page
            if (response != null)
            {
                string body = new StreamReader(response.GetResponseStream()).ReadToEnd();

                // Insert image with request to the static page if Analytics is enabled.
                // This is a hotfix for a Sitecore bug, see Sitecore issue #378950
                if (Settings.GetBoolSetting("Xdb.Enabled", false) && site.Tracking().EnableTracking)
                {
                    body = body.Replace("</body>",
                        string.Format("<img src=\"{0}?{1}\" height=\"1\" width=\"1\" border=\"0\"></body>",
                            Settings.GetSetting(this.SettingsKey + ".Static"),
                            base.Request.QueryString));
                }

                this.Response.Write(body);
            }
            else
            {
                this.Response.Write("Statuscode: " + this.StatusCode);
            }

            // set statuscode
            if (this.StatusCode > 0)
            {
                this.Response.StatusCode = this.StatusCode;
            }

            // pass through the response we create here
            this.Response.TrySkipIisCustomErrors = true;
        }

        protected virtual NetworkCredential GetBasicAuthenticationCredentials()
        {
            var username = Settings.GetSetting("ErrorManager.BasicAuthentication.Username");
            var password = Settings.GetSetting("ErrorManager.BasicAuthentication.Password");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ConfigurationException("The username and/or password for using Basic Authentication is not defined in the settings");
            }

            return new NetworkCredential(username, password);
        }

        private void HandleHeaderForwarding(HttpWebRequest request)
        {
            var headers = Settings.GetSetting("ErrorManager.ForwardedHeaders");
            if (string.IsNullOrWhiteSpace(headers)) return;

            foreach (var header in headers.Split('|'))
            {
                if (this.Request.Headers[header] != null)
                {
                    request.Headers.Add(header, this.Request.Headers[header]);
                }
            }
        }

        private void HandleBasicAuthentication(HttpWebRequest request)
        {
            if (!Settings.GetBoolSetting("ErrorManager.BasicAuthentication.Enabled", false)) return;

            request.Credentials = this.GetBasicAuthenticationCredentials();
        }

        private void AddRequestCookies(HttpWebRequest request)
        {
            var excludedCookieNames =
                Settings.GetSetting("ErrorManager.ExcludedCookies", "ASP.NET_SessionId")
                    .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            request.CookieContainer = new CookieContainer();
            var requestCookies = this.Request.Cookies;

            for (var cookieCounter = 0; cookieCounter < requestCookies.Count; cookieCounter++)
            {
                var requestCookie = requestCookies.Get(cookieCounter);

                // Ignore all excluded cookies
                if (excludedCookieNames.Contains(requestCookie.Name)) continue;

                var forwardedCookie = new Cookie
                {
                    /*  We have to add the target host because the cookie does not contain the domain information.
                    In this case, this behaviour is not a security issue, because the target is our own platform.
                    Further informations: http://stackoverflow.com/a/460990 
                    */
                    Domain = request.RequestUri.Host,
                    Expires = requestCookie.Expires,
                    Name = requestCookie.Name,
                    Path = requestCookie.Path,
                    Secure = requestCookie.Secure,
                    Value = requestCookie.Value
                };

                request.CookieContainer.Add(forwardedCookie);
            }
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
