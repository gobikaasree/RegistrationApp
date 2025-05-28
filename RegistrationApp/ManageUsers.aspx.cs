using Microsoft.Reporting.WebForms;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RegistrationApp
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUsers();
            }
        }


        protected void btnAddUser_Click(object sender, EventArgs e)
        {
            string fileName = "";

            if (fuDocument.HasFile)
            {
                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(fuDocument.FileName);
                string filePath = Server.MapPath("~/Uploads/" + fileName);
                fuDocument.SaveAs(filePath);
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                SqlCommand enableCmd = new SqlCommand("SET IDENTITY_INSERT Users ON", con);
                enableCmd.ExecuteNonQuery();

                string query = "INSERT INTO Users (Id, FullName, Email, PhoneNumber, DocumentPath) VALUES (@Id, @FullName, @Email, @PhoneNumber, @DocumentPath)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", int.Parse(txtId.Text));
                cmd.Parameters.AddWithValue("@FullName", txtName.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text);
                cmd.Parameters.AddWithValue("@DocumentPath", fileName);
                cmd.ExecuteNonQuery();

                SqlCommand disableCmd = new SqlCommand("SET IDENTITY_INSERT Users OFF", con);
                disableCmd.ExecuteNonQuery();

                con.Close();
            }

            LoadUsers();

            txtId.Text = "";
            txtName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            fuDocument.Attributes.Clear();
        }



        void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Users ORDER BY Id", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        protected void gvUsers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvUsers.EditIndex = e.NewEditIndex;
            LoadUsers();

            // Get user ID
            int userId = Convert.ToInt32(gvUsers.DataKeys[e.NewEditIndex].Value);

            // Find the Repeater in the GridView row
            GridViewRow row = gvUsers.Rows[e.NewEditIndex];
            Repeater rptEditDocs = (Repeater)row.FindControl("rptEditDocs");

            if (rptEditDocs != null)
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM UserDocuments WHERE UserId = @UserId", con);
                    da.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    rptEditDocs.DataSource = dt;
                    rptEditDocs.DataBind();
                }
            }
        }

        protected void gvUsers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvUsers.EditIndex = -1;
            LoadUsers();
        }

        protected void gvUsers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvUsers.DataKeys[e.RowIndex].Value);
            string fullName = ((TextBox)gvUsers.Rows[e.RowIndex].Cells[1].Controls[0]).Text;
            string email = ((TextBox)gvUsers.Rows[e.RowIndex].Cells[2].Controls[0]).Text;
            string phone = ((TextBox)gvUsers.Rows[e.RowIndex].Cells[3].Controls[0]).Text;

            FileUpload fuEditDocument = (FileUpload)gvUsers.Rows[e.RowIndex].FindControl("fuEditDocument");
            string newFileName = GetExistingFilePath(id); // default: retain existing

            if (fuEditDocument != null && fuEditDocument.HasFile)
            {
                // Delete old file
                string oldFilePath = Server.MapPath("~/Uploads/" + newFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Save new file
                newFileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(fuEditDocument.FileName);
                string newFilePath = Server.MapPath("~/Uploads/" + newFileName);
                fuEditDocument.SaveAs(newFilePath);
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET FullName=@FullName, Email=@Email, PhoneNumber=@Phone, DocumentPath=@DocPath WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@DocPath", newFileName);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            gvUsers.EditIndex = -1;
            LoadUsers();
        }



        protected void gvUsers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvUsers.DataKeys[e.RowIndex].Value);

            string existingFile = GetExistingFilePath(id);

            if (!string.IsNullOrEmpty(existingFile))
            {
                string filePath = Server.MapPath("~/Uploads/" + existingFile);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadUsers();
        }


        protected void btnShowUsers_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
            LoadUsers();
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 1;
            LoadReport();
        }

        void LoadReport()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Users", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Report/Report1.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dt));
                ReportViewer1.LocalReport.Refresh();
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SendMail")
            {
                string email = e.CommandArgument.ToString();
                SendEmail(email);
            }
            else if (e.CommandName == "UploadDoc")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                lblUploadUserId.Text = userId.ToString();
                pnlUpload.Visible = true;
                LoadDocuments(userId);
            }
        }

        private void SendEmail(string toEmail)
        {
            try
            {
                string fromEmail = ConfigurationManager.AppSettings["SMTPEmail"];
                string password = ConfigurationManager.AppSettings["SMTPPassword"];
                string smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
                bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSSL"]);

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = "Test Email from RegistrationApp";
                mail.Body = "Hello student,\n\nThis is a test email from your registration system.\n\nRegards,\nAdmin";

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(fromEmail, password);
                smtp.EnableSsl = enableSsl;

                smtp.Send(mail);

                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Email sent to {toEmail}');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Failed to send email: {ex.Message}');", true);
            }
        }

        protected void btnCancelUpload_Click(object sender, EventArgs e)
        {
            pnlUpload.Visible = false;
        }

        private string GetExistingFilePath(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT DocumentPath FROM Users WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }
        protected void btnUploadMultiDocs_Click(object sender, EventArgs e)
        {
            int userId = int.Parse(lblUploadUserId.Text);

            if (fuMultiDocs.HasFiles)
            {
                foreach (HttpPostedFile file in fuMultiDocs.PostedFiles)
                {
                    string fileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    string filePath = Server.MapPath("~/Uploads/" + fileName);
                    file.SaveAs(filePath);

                    using (SqlConnection con = new SqlConnection(cs))
                    {
                        SqlCommand cmd = new SqlCommand("INSERT INTO UserDocuments (UserId, FileName) VALUES (@UserId, @FileName)", con);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            LoadDocuments(userId);
        }
        private void LoadDocuments(int userId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM UserDocuments WHERE UserId = @UserId", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                rptDocuments.DataSource = reader;
                rptDocuments.DataBind();
            }
        }
        protected void rptDocuments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteDoc")
            {
                int docId = int.Parse(e.CommandArgument.ToString());

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    SqlCommand getCmd = new SqlCommand("SELECT FileName FROM UserDocuments WHERE Id = @Id", con);
                    getCmd.Parameters.AddWithValue("@Id", docId);
                    string fileName = (string)getCmd.ExecuteScalar();

                    string filePath = Server.MapPath("~/Uploads/" + fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    SqlCommand delCmd = new SqlCommand("DELETE FROM UserDocuments WHERE Id = @Id", con);
                    delCmd.Parameters.AddWithValue("@Id", docId);
                    delCmd.ExecuteNonQuery();
                }
                gvUsers.EditIndex = -1;
                LoadUsers();
            }
        }
        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int userId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Id"));
                Repeater rptInlineDocs = (Repeater)e.Row.FindControl("rptInlineDocuments");

                if (rptInlineDocs != null)
                {
                    using (SqlConnection con = new SqlConnection(cs))
                    {
                        SqlCommand cmd = new SqlCommand(@"
    SELECT FileName FROM UserDocuments WHERE UserId = @UserId
    UNION
    SELECT DocumentPath AS FileName FROM Users WHERE Id = @UserId AND ISNULL(DocumentPath, '') <> ''
", con);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptInlineDocs.DataSource = dt;
                        rptInlineDocs.DataBind();

                    }
                }
            }
        }

    }
}