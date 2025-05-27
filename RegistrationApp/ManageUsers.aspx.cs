using Microsoft.Reporting.WebForms;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
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
            string newFileName = GetExistingFilePath(id); 

            if (fuEditDocument != null && fuEditDocument.HasFile)
            {
                string oldFilePath = Server.MapPath("~/Uploads/" + newFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

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
        protected void btnUploadDoc_Click(object sender, EventArgs e)
        {
            int userId = int.Parse(lblUploadUserId.Text);
            if (fuSingleUpload.HasFile)
            {
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(fuSingleUpload.FileName);
                string filePath = Server.MapPath("~/Uploads/" + fileName);
                fuSingleUpload.SaveAs(filePath);

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Users SET DocumentPath=@DocPath WHERE Id=@Id", con);
                    cmd.Parameters.AddWithValue("@DocPath", fileName);
                    cmd.Parameters.AddWithValue("@Id", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                pnlUpload.Visible = false;
                LoadUsers();
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

    }
}