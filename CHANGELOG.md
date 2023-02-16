# 8.1.4

- Fix potential SSRF vulnerability

# 8.1.3

- Fix unwanted white space on not found pages due to img element not being hidden

# 8.1.2

- More logging on server error

# 8.1.1

- Fix issue #16 where the HttpContext in the CheckErrorManagerUserAgent processor inherits the lifetime scope of the processor, resulting in flagging every visitor as bot and with this, disabling personalization on all components.

# 8.1

- New feature: Support for personalization on error pages. To enable this feature, you need to set the "ErrorManager.DisableTracking" setting to false.
- Allow different NotFoundUrl between media requests and non-media requests
- Introduce MediaNotFoundUrl.UseStatic as preset for static url

# 8.0.3

- suppress infinite loop by checking target URL and requested ones, if "AbsolutePath" is same, go to layout static page
- add support to all versions of TLS

# 8.0.2

- fix many sessions problem caused by edit mode, which prevented users from logging in
- introduce StringBuilder and fix minor code readability issues 

# 8.0.1

- fix exception for items from Media Library not being a file, thus not having Blob field
- handle other exceptions during media item check with "not found" result
- add DotNetCompilerPlatform to enable C# 6 compilation on TeamCity

# 8.0

- integrate the new Sitecore NuGet feed and reference the relevant NuGet packages from there
- target Sitecore 8.1 update 1 as the minimum supported version (Introduction of the PageMode.IsExperienceEditor flag)
- replace the Sitecore Analytics integration with the new xDB API
- introduce support for basic authentication
- introduce support for forwarding configured headers
- introduce support for excluding specific cookies from forwarding to the error page
- introduce support for disabling the xDB tracker for requests initiated by the error manager
- fix a Sitecore bug with item language fallback introduced with Sitecore 8.1 update 3 
