<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="RegistrationApp.ManageUsers" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Student Registration</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" class="container mt-4">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <asp:Button ID="btnShowUsers" runat="server" CssClass="btn btn-secondary mb-2" Text="Manage Users" OnClick="btnShowUsers_Click" />
        <asp:Button ID="btnShowReport" runat="server" CssClass="btn btn-info mb-2 ms-2" Text="View Report" OnClick="btnShowReport_Click" />

        <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
            <asp:View ID="viewUsers" runat="server">
                <h2>User List</h2>
                <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" CssClass="table table-bordered table-striped"
                    OnRowEditing="gvUsers_RowEditing"
                    OnRowUpdating="gvUsers_RowUpdating"
                    OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                    OnRowDeleting="gvUsers_RowDeleting"
                    OnRowCommand="gvUsers_RowCommand"
                    OnRowDataBound="gvUsers_RowDataBound">
                    
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="True" />
                        <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="PhoneNumber" HeaderText="Phone Number" />
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                       <asp:TemplateField HeaderText="Actions">
    <ItemTemplate>
        <asp:Button ID="btnSendMail" runat="server" Text="Send Mail"
            CommandName="SendMail"
            CommandArgument='<%# Eval("Email") %>' CssClass="btn btn-success btn-sm" />

        <asp:LinkButton ID="lnkUpload" runat="server" Text="Upload Doc"
            CommandName="UploadDoc"
            CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-warning btn-sm" />

        <asp:Repeater ID="rptInlineDocuments" runat="server">
    <ItemTemplate>
        <a href='<%# ResolveUrl("~/Uploads/" + Eval("FileName")) %>' target="_blank" class="btn btn-link btn-sm d-block" download='<%# Eval("FileName") %>'>
            <%# Eval("FileName") %>
        </a>
    </ItemTemplate>
</asp:Repeater>
    </ItemTemplate>

    <EditItemTemplate>
    <asp:Repeater ID="rptEditDocs" runat="server" OnItemCommand="rptDocuments_ItemCommand">
        <ItemTemplate>
            <div class="d-flex align-items-center justify-content-between mb-1">
                <a href='<%# "~/Uploads/" + Eval("FileName") %>' target="_blank"><%# Eval("FileName") %></a>
                <asp:LinkButton ID="lnkDeleteDoc" runat="server" Text="Delete"
                    CommandArgument='<%# Eval("Id") %>' CommandName="DeleteDoc"
                    CssClass="btn btn-sm btn-danger ms-2" />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</EditItemTemplate>

</asp:TemplateField>

                    </Columns>

                </asp:GridView>

                <h3>Add New User</h3>
                <asp:TextBox ID="txtId" runat="server" CssClass="form-control mb-2" Placeholder="User ID" />
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control mb-2" Placeholder="Full Name" />
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control mb-2" Placeholder="Email" />
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control mb-2" Placeholder="Phone Number" />
                <asp:FileUpload ID="fuDocument" runat="server" CssClass="form-control mb-2" />
                <asp:Button ID="btnAddUser" runat="server" Text="Add User" CssClass="btn btn-primary" OnClick="btnAddUser_Click" />
<asp:Panel ID="pnlUpload" runat="server" Visible="false" CssClass="mt-3 border p-3">
    <h5>Upload Documents for User ID: <asp:Label ID="lblUploadUserId" runat="server" /></h5>

    <asp:FileUpload ID="fuMultiDocs" runat="server" CssClass="form-control mb-2" AllowMultiple="true" />
    <asp:Button ID="btnUploadMultiDocs" runat="server" Text="Upload Files" CssClass="btn btn-primary mb-2" OnClick="btnUploadMultiDocs_Click" />
    <asp:Button ID="btnCancelUpload" runat="server" Text="Cancel" CssClass="btn btn-secondary mb-2" OnClick="btnCancelUpload_Click" />

    <asp:Repeater ID="rptDocuments" runat="server" OnItemCommand="rptDocuments_ItemCommand">
        <ItemTemplate>
            <div class="d-flex align-items-center justify-content-between mb-1">
                <a href='<%# "~/Uploads/" + Eval("FileName") %>' target="_blank"><%# Eval("FileName") %></a>
                <asp:LinkButton ID="lnkDelete" runat="server" Text="Delete"
                    CommandArgument='<%# Eval("Id") %>' CommandName="DeleteDoc"
                    CssClass="btn btn-sm btn-danger ms-2" />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>


            </asp:View>

            <asp:View ID="viewReport" runat="server">
                <h2 class="text-center">Students Registration Report</h2>
                <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" Height="600px" ProcessingMode="Local" />
            </asp:View>
        </asp:MultiView>
    </form>
</body>
</html>