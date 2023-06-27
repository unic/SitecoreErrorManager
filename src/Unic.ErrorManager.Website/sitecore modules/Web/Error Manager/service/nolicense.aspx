<%@ OutputCache Location="None" VaryByParam="none" %>
<%@ register TagPrefix="sc" Namespace="Sitecore.Web.UI.HtmlControls" Assembly="Sitecore.Kernel" %>
<%@ Page language="c#" Codebehind="nolicense.aspx.cs" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="True" Inherits="SitecoreClient.Page.NoLicense" %>
<% Response.StatusCode = 200; %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
  <head>
    <title>License Missing</title>
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
          <sc:ThemedImage runat="server" Src="Network/48x48/certificate_error.png" Width="48" Height="48" Align="absmiddle" Margin="0px 8px 0px 0px"/>
          A required license is missing
        </div>
      </div>
      
      <div id="ErrorPanel">
        Most likely causes:
        <ul>
          <li>The resource you are trying to access requires the following license:
              <asp:label id="RequiredLicense" runat="server">[license name]</asp:label>.</li>
        </ul>
      
        <div class="ErrorOptions">What you can try:</div>
        
        <div>
          <a class="ErrorOption" href="javascript:history.go(-1)">
            <sc:ThemedImage runat="server" Src="Applications/16x16/bullet_ball_blue.png" Width="16" Height="16" Align="middle" Margin="0px 4px 0px 4px" />
            Go back to the previous page
          </a>
        </div>
        
        <div>
          <a class="ErrorOption" href="javascript:location.href='/'">
            <sc:ThemedImage runat="server" Src="Applications/16x16/bullet_ball_blue.png" Width="16" Height="16" Align="middle" Margin="0px 4px 0px 4px" />
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
          
          <asp:placeholder id="Information" runat="server" />
        </div>
        
      </div>
    </div>
  </body>
</html>
