<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageBrowser.aspx.cs" Inherits="OneMainWeb.ImageBrowser" %>
<%@ Register TagPrefix="one" TagName="FileManager" Src="~/AdminControls/FileManager.ascx" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
     <link href="/Utils/one.css" type="text/css" rel="stylesheet" media="all" title="one default" />
    <script type="text/javascript" src="/JavaScript/one.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="fmPopUp">
        <one:FileManager runat="server" FilterExtensions="gif,jpg,jpeg,png,tif,.gif,.jpg,.jpeg,.png,.tif" ID="filemng" ShowSelectLink="true" ContainerControlType="fck" />    
    </div>
    </form>
</body>
</html>



