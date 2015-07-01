<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Clear.aspx.cs" Inherits="OneMainWeb.Utils.Clear" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Clear cache</title>
    <meta name="robots" content="noindex,nofollow" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Clear cache" />
            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Clear cookies" />
            <asp:Label ID="Label1" runat="server" Text=""></asp:Label>

            <asp:Literal ID="LiteralCms" runat="server" Text=""></asp:Literal>
        </div>
    </form>
</body>
</html>
