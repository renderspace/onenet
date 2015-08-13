<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OneClear.aspx.cs" Inherits="OneMainWeb.Utils.OneClear" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="/adm/css/bootstrap.css" rel="stylesheet" />

</head>
<body style="background-color: #2e3d4c;">
    <form id="form1" runat="server">
    <div>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Clear cache" Cssclass="btn btn-default btn-sm" />
        <asp:Label ID="Label1" runat="server" style="color: white;"></asp:Label>
    </div>
    </form>
</body>
</html>
