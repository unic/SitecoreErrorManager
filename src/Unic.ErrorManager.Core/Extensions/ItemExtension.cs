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

namespace Unic.ErrorManager.Core.Extensions
{
    using System.Linq;
    using System.Web;

    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Globalization;

    /// <summary>
    /// Class with extension methods for class <see cref="Sitecore.Data.Items.Item"/>.
    /// </summary>
    /// <author>Kevin Brechbühl - Unic AG</author>
    public static class ItemExtension
    {
        /// <summary>
        /// Extension method to check if the item has at least one version in a given language.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="language">The language.</param>
        /// <param name="availableLanguages">Comma-separated list of available languages. If the given language is not in this list, method returns <c>false</c>. Leave empty for accepting all languages.</param>
        /// <returns><c>True</c> if item has a valid language version, <c>false</c> otherwise.</returns>
        public static bool HasLanguageVersion(this Item item, Language language, string availableLanguages)
        {
            Item itemInLang = ItemManager.GetItem(item.ID, language, Sitecore.Data.Version.Latest, item.Database);
            if (itemInLang != null && itemInLang.Versions.GetVersions().Length > 0)
            {
                if (string.IsNullOrEmpty(availableLanguages)
                    || (HttpContext.Current != null && HttpContext.Current.Request.QueryString["em_force"] == "true")
                    || availableLanguages.ToLower().Split(',').Contains<string>(itemInLang.Language.Name.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
