using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RegistrationApp
{
    public partial class Register : System.Web.UI.Page
    {
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (password != confirmPassword)
            {
                lblMessage.Text = "Passwords do not match.";
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["RegistrationConnection"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password); // In real apps, use hashing!

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = "Registration successful and saved to database!";
        }
    }
}