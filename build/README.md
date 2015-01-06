# How to Build the Solution

- Make sure you have added the latest .Net 4.0 DLLs in the lib folder (we use latest
  Sitecore 6.6 release)
- If you wish to deploy the module to a local playground for testing, you can use
  the VS _Publish_ feature.
  (Open the Solution, right-click on the Unic.ErrorManager.Website
  Project and select "Publish...", define the webroot of your playground as
  target location.)
- Check [installation instructions] (https://github.com/unic/SitecoreErrorManager/wiki/Installation) for further steps required in your playground to fully test Sitecore Error Manager.

---

**Note**: _We use a private NuGet feed to manage the Sitecore dependencies. This should not be an issue for you, though. We disabled automatic package restore for the project and added the ./lib directory as an alternative source to check for referenced libraries. So you should be able to build and run the module without issues once you've completed the steps above._

---
