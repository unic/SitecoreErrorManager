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
using Sitecore.Web;
using Unic.ErrorManager.Core.Definitions;

namespace Unic.ErrorManager.Website.sitecore_modules.Web.Error_Manager
{
    using Unic.ErrorManager.Core.Controls;

    /// <summary>
    /// Page for status code 404 (notfound). It inherits from the <see cref="BaseError"/>, which does all needed stuff.
    /// </summary>
    /// <author>Kevin Brechbühl - Unic AG</author>
    public partial class _404 : BaseError
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event and sets the settings key and status code for the current error page.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.SettingsKey = this.IsMediaRequest() ? "MediaNotFoundUrl" : "ItemNotFoundUrl";
            base.StatusCode = 404;

            base.OnLoad(e);
        }

        protected virtual bool IsMediaRequest()
        {
            var isMedia = this.Context.Items[Constants.IsMediaParameterName];
            if (isMedia is bool isMediaRequest)
            {
                return isMediaRequest;
            }

            var isMediaQueryString = WebUtil.GetQueryString(Constants.IsMediaParameterName);
            return bool.TryParse(isMediaQueryString, out var isMediaQuery) && isMediaQuery;
        }
    }
}