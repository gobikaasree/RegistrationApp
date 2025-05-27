<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="RegistrationApp.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>User Registration</h2>

<table>
    <tr>
        <td>Full Name:</td>
        <td><asp:TextBox ID="txtName" runat="server" /></td>
    </tr>
    <tr>
        <td>Email:</td>
        <td><asp:TextBox ID="txtEmail" runat="server" /></td>
    </tr>
    <tr>
        <td>Password:</td>
        <td><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" /></td>
    </tr>
    <tr>
        <td>Confirm Password:</td>
        <td><asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" /></td>
    </tr>
    <tr>
        <td colspan="2">
            <asp:Button ID="btnRegister" runat="server" Text="Register" OnClick="btnRegister_Click" />
        </td>
    </tr>
</table>

<asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

        </div>
    </form>
</body>
</html>
