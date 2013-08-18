using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Contacts;
using Google.GData.Contacts;
using Google.GData.Client;
using Google.GData.Extensions;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void loginButton_Click(object sender, EventArgs e)
    {
        //We will test the users credentials by attempting a data extraction from the address book.
        //The data extraction will be small since we set autopaging to false. This will ensure a good efficiency.
        RequestSettings settings = new RequestSettings("ProQuorum Messaging Module", userName.Text, password.Text);
        settings.AutoPaging = false;
        ContactsRequest cr = new ContactsRequest(settings);
        Feed<Contact> f = cr.GetContacts();
        try
        {
            foreach (Contact entry in f.Entries)
            {
                break;
            }
            //If the application is able to reach this point then the credentials are valid.
            //Set the session variables so the user does not have to keep logging in.
            Session["username"] = userName.Text;
            Session["password"] = password.Text;
            Response.Redirect("http://localhost:1172/messaging.aspx");
        }
        catch (Exception ex)
        {
            Output.Text = "Invalid Username and Password combination. Please try again.";
        }
    }
}