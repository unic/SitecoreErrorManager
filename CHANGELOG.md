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