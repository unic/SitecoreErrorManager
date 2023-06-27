<%@ OutputCache Location="None" VaryByParam="none" %>
<%@ register TagPrefix="sc" Namespace="Sitecore.Web.UI.HtmlControls" Assembly="Sitecore.Kernel" %>
<%@ Page language="c#" Codebehind="nolayout.aspx.cs" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="True" Inherits="SitecoreClient.Page.NoLayout" %>
<% Response.StatusCode = 200; %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
  <head>
    <title>Layout Not Found</title>
    <link href="/sitecore/login/default.css" rel="stylesheet" />
    <script type="text/javascript" language="javascript">
    
    function toggleMore() {
      var more = document.getElementById("ErrorMorePanel");
      more.style.display = more.style.display == "none" ? "" : "none";
    }
    
    </script>
  </head>
  <body>
    <div id="Body">
      <div id="ErrorTopPanel">
        <div class="ErrorTitle">
          <sc:ThemedImage runat="server" Src="Applications/48x48/window_error.png" Width="48" Height="48" Align="middle" Margin="0px 8px 0px 0px" Float="left"/>
          The layout for the requested document<br />was not found.
        </div>
      </div>
      
      <div id="ErrorPanel">
        Most likely causes:
        <ul>
          <li>The resource you are looking for (or one of its 
              dependencies) may have been removed, had its name changed, or is temporarily 
              unavailable. Please review the following URL and make sure that it is spelled 
              correctly.</li>
        </ul>
      
        <div class="ErrorOptions">What you can try:</div>
        
        <div>
          <a class="ErrorOption" href="javascript:history.go(-1)">
            <sc:ThemedImage runat="server" Src="Applications/16x16/bullet_ball_blue.png" Width="16" Height="16" Align="top" Padding="1px 0px 0px 0px" Margin="0px 4px 0px 4px" />
            Go back to the previous page
          </a>
        </div>
        
        <div>
          <a class="ErrorOption" href="javascript:location.href='/'">
            <sc:ThemedImage runat="server" Src="Applications/16x16/bullet_ball_blue.png" Width="16" Height="16" Align="top" Padding="1px 0px 0px 0px" Margin="0px 4px 0px 4px" />
            Go to the start page
          </a>
        </div>
        
        <div class="ErrorMoreOptionPanel">
          <a class="ErrorOption" href="javascript:toggleMore()">
            <sc:ThemedImage id="MoreGlyph" runat="server" Src="Applications/16x16/navigate_close.png" Width="16" Height="16" Align="middle" Margin="0px 4px 0px 4px" />
            More information
          </a>
        </div>
        
        <div id="ErrorMorePanel" style="display:none">
          <div>
            Requested URL: <asp:Placeholder id="RequestedUrl" runat="server" />
          </div>
          <div>
            Requested Layout: <asp:Placeholder id="RequestedLayout" runat="server" />
          </div>
          <div>
            Requested Device: <asp:Placeholder id="RequestedDevice" runat="server" />
          </div>
          <div>
            If the page you are trying to display exists, please check that an appropriate prefix has been added to the IgnoreUrlPrefixes setting in the web.config.
          </div>
        </div>
        
      </div>
    </div>
  </body>
</html>
