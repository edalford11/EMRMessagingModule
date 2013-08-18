using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Contacts;
using Google.GData.Contacts;
using Google.GData.Client;
using Google.GData.Extensions;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net.Sockets;
using System.Net.Security;
using System.Globalization;
using System.Threading;

public partial class Default2 : System.Web.UI.Page
{
    static TcpClient tcpc = null;
    static SslStream ssl = null;
    static byte[] dummy;
    static byte[] buffer;
    static int bytes = -1;
    int numEmails = 0;

    Contact contact;
    bool editContactFlag = false;
    String accessToken = "";
    String code = "";
    //Data source for the address book list box.
    List<string> _contactNames = new List<string>();
    List<Contact> _contacts = new List<Contact>();  //We need have a list of type Contact so we can reference it for editing and deleting.

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["username"] == null || Session["password"] == null)
        {
            /*oAuth 2.0 Legacy
            code = Request.QueryString["code"]; //extract oAUTH code from the URL that google has provided us
            accessToken = getToken();
            Session["accessToken"] = accessToken; //Save access token for all future interactions with google for this session.
            emailInfo.Text = "New Access Token: " + accessToken;
            */
            //Redirect the user to the login page if these variables are not set
            Response.Redirect("http://localhost:1172/Default.aspx");
        }
        else
        {
            /*oAuth 2.0 Legacy
            accessToken = (String)Session["accessToken"];
            emailInfo.Text = "Same Session Token: " + accessToken;
            */

        }

        //Extract the contacts from the address book.
        RequestSettings settings = new RequestSettings("ProQuorum Messaging Module", (String)Session["username"], (String)Session["password"]);
        //Setting autopaging to true means all of the contacts will be extracted instead of a portion.
        settings.AutoPaging = true;
        ContactsRequest cr = new ContactsRequest(settings);
        Feed<Contact> f = cr.GetContacts();

        //Input all names into a list, which will be used as the data source for the ListBox.
        foreach (Contact entry in f.Entries)
        {
            _contacts.Add(entry);
            if (entry.Name != null)
            {
                Name name = entry.Name;
                if (!string.IsNullOrEmpty(name.FullName))
                    _contactNames.Add(name.FullName);
                else
                    _contactNames.Add("No full name found.");
            }
            else
                _contactNames.Add("No name found.");
            /*
            foreach (EMail email in entry.Emails)
            {
                _emails.Add(email.Address);
            }*/
        }

        //Sort both the lists of contacts in alphabetical order
        _contactNames.Sort();
        _contacts = _contacts.OrderBy(o => o.Name.FullName).ToList();
        //Set the title label on top of the address book to the number of contacts.
        title.Text = "Contact List (" + _contacts.Count.ToString() + " entries)";
        if (!Page.IsPostBack)  //this needs to be checked so that the listbox can read what index the user is selecting.
        {
            ContactsListBox.DataSource = _contactNames;
            ContactsListBox.DataBind();
        }

        //Populate the inbox with the emails
        try
        {
            tcpc = new System.Net.Sockets.TcpClient("imap.gmail.com", 993);
            ssl = new System.Net.Security.SslStream(tcpc.GetStream());
            ssl.AuthenticateAsClient("imap.gmail.com"); 
            receiveResponse("");
            //IMAP login command
            string googleResponse = receiveResponse("$ LOGIN " + (String)Session["username"] + " " + (String)Session["password"] + "  \r\n");
            System.Diagnostics.Debug.WriteLine(googleResponse + " |LOGIN END|");
            //This command lists the folders (inbox,sentmail,users labels )
            //receiveResponse("$ LIST " + "\"\"" + " \"*\"" + "\r\n");
            //Select the inbox folder
            googleResponse = receiveResponse("$ SELECT INBOX\r\n");
            System.Diagnostics.Debug.WriteLine(googleResponse + " |SELECT INBOX END|");
            //Get the status of the inbox. Response includes the number of messages.
            googleResponse = receiveResponse("$ STATUS INBOX (MESSAGES)\r\n");
            System.Diagnostics.Debug.WriteLine(googleResponse + " |STATUS INBOX END|");
            //Use String.Split to extract the number of emails and parses.
            string[] stringSeparators = new string[] { "(MESSAGES", ")" };
            string[] words = googleResponse.ToString().Split(stringSeparators, StringSplitOptions.None);
            
            try{
                numEmails = int.Parse(words[1]);
            }
            catch
            {
                MessageBox.Show("Error parsing number of emails.");
            }
            
            //Extract all emails and organize them into the table.
            if (numEmails > 0)
            {
                for (int i = numEmails; i >= 1; i--)
                {
                    TableRow r = new TableRow();
                    //Highlight when cursor is over the message.
                    r.Attributes["onmouseover"] = "highlight(this, true);";
                    //Remove the highlight when the curser exits the message
                    r.Attributes["onmouseout"] = "highlight(this, false);";
                    r.Attributes["style"] = "cursor:pointer;";
                    r.Attributes["onclick"] = "link(this, false);";

                    r.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");

                    //The first cell in the row will be a checkbox
                    TableCell c0 = new TableCell();
                    c0.HorizontalAlign = HorizontalAlign.Center;
                    System.Web.UI.WebControls.CheckBox checkBox = new System.Web.UI.WebControls.CheckBox();
                    //Add the js function to the checkbox that will highlight the row when checked and unhighlight when unchecked.
                    checkBox.Attributes.Add("onclick", "javascript:colorRow(this);");
                    c0.Controls.Add(checkBox);
                    r.Cells.Add(c0);

                    //Specifc email number to fetch.
                    googleResponse = receiveResponse("$ FETCH " + i + " body[HEADER.FIELDS (DATE FROM SUBJECT)]\r\n");
                    while (!googleResponse.Contains("$ OK Success"))
                        googleResponse += receiveResponse("");
                    System.Diagnostics.Debug.WriteLine(googleResponse + " |HEADER "+i+" END|");
                    
                    string[] orderedHeaders = organizeHeaders(googleResponse);

                    //The second cell will be who the message is from (email)
                    TableCell c1 = new TableCell();
                    //c1.Controls.Add(new LiteralControl(headers[3]));
                    c1.Controls.Add(new LiteralControl(orderedHeaders[0]));
                    r.Cells.Add(c1);
                        
                    //The third cell will be the subject.
                    TableCell c2 = new TableCell();
                    c2.Controls.Add(new LiteralControl(orderedHeaders[1]));
                    r.Cells.Add(c2);
                        
                    //The fourth cell will be the Date.
                    TableCell c3 = new TableCell();
                    //Parse the date into a more readable format
                    string[] ss = new string[] { " " };
                    string[] dateSplit = orderedHeaders[2].Split(ss, StringSplitOptions.None);
                    DateTime time = Convert.ToDateTime(dateSplit[5]);
                    c3.Controls.Add(new LiteralControl(dateSplit[1] + " " + dateSplit[2] + " " + dateSplit[3] + " " + dateSplit[4] + " " + time.ToShortTimeString()));
                    r.Cells.Add(c3);
                        
                    googleResponse = receiveResponse("$ FETCH " + i + " body[text]\r\n");
                    System.Threading.Thread.Sleep(1000);
                    googleResponse += receiveResponse("");
                    System.Diagnostics.Debug.WriteLine(googleResponse + " |MESSAGE "+i+" END|");
                    //Remove the beginning metadata
                    int beginningDataIndex = googleResponse.IndexOf("}");
                    if(beginningDataIndex!=-1)
                        googleResponse = googleResponse.Remove(0, beginningDataIndex+1);
                    googleResponse = googleResponse.Trim();
                    //Remove the ") $ OK Success" at the end of the header
                    int index = googleResponse.LastIndexOf(")");
                    if (index > -1)
                        googleResponse = googleResponse.Remove(index);

                    //Add a hidden cell so the javascript can access the message and put it in a modal when clicked on.
                    TableCell c4 = new TableCell();
                    //c4.Controls.Add(new LiteralControl(emailBody[1]));
                    c4.Controls.Add(new LiteralControl(googleResponse.ToString()));
                    r.Cells.Add(c4);
                    c4.Attributes.Add("hidden", "true");

                    InboxTable.Rows.Add(r);
                }
            }
            else
            {
                TableRow r = new TableRow();
                TableCell c1 = new TableCell();
                c1.Controls.Add(new LiteralControl("No Messages"));
                r.Cells.Add(c1);
                InboxTable.Rows.Add(r);
            }
            receiveResponse("$ LOGOUT\r\n");
        }
        catch (Exception ex)
        {
            MessageBox.Show("error fetching emails: " + ex.Message);
        }
        finally
        {
            if (ssl != null)
            {
                ssl.Close();
                ssl.Dispose();
            }
            if (tcpc != null)
            {
                tcpc.Close();
            }
        }
    }

    private string[] organizeHeaders(string strParse)
    {
        string[] orderedHeaders = new string[3];

        //Remove the ") $ OK Success" at the end of the header
        int index = strParse.LastIndexOf(")");
        if (index > -1)
            strParse = strParse.Remove(index);

        //Remove the beginning metadata
        strParse = strParse.Remove(0, 57);
        strParse = strParse.Trim();

        string[] stringSeparators = new string[] { "From:", "Date:", "Subject:" };
        string[] headers = strParse.ToString().Split(stringSeparators, StringSplitOptions.None);
        int fromIndex = strParse.IndexOf("From:");
        int dateIndex = strParse.IndexOf("Date:");
        int subjIndex = strParse.IndexOf("Subject:");

        if (fromIndex < dateIndex && dateIndex < subjIndex)
        {
            orderedHeaders[0] = headers[1];//From
            orderedHeaders[1] = headers[3];//Subject
            orderedHeaders[2] = headers[2];//Date
        }
        else if (fromIndex < subjIndex && subjIndex < dateIndex)
        {
            orderedHeaders[0] = headers[1];
            orderedHeaders[1] = headers[2];
            orderedHeaders[2] = headers[3];
        }
        else if (dateIndex < fromIndex && fromIndex < subjIndex)
        {
            orderedHeaders[0] = headers[2];
            orderedHeaders[1] = headers[3];
            orderedHeaders[2] = headers[1];
        }
        else if (subjIndex < dateIndex && dateIndex < fromIndex)
        {
            orderedHeaders[0] = headers[3];
            orderedHeaders[1] = headers[1];
            orderedHeaders[2] = headers[2];
        }
        else if (dateIndex < subjIndex && subjIndex < fromIndex)
        {
            orderedHeaders[0] = headers[3];
            orderedHeaders[1] = headers[2];
            orderedHeaders[2] = headers[1];
        }
        else
        {//(subjIndex<fromIndex && fromIndex<dateIndex){
            orderedHeaders[0] = headers[2];
            orderedHeaders[1] = headers[1];
            orderedHeaders[2] = headers[3];
        }

        return orderedHeaders;
    }

    private string receiveResponse(string command)
    {
        try
        {
            if (command != "")
            {
                if (tcpc.Connected)
                {
                    dummy = Encoding.ASCII.GetBytes(command);
                    ssl.Write(dummy, 0, dummy.Length);
                }
                else
                {
                    MessageBox.Show("TCP CONNECTION DISCONNECTED");
                }
            }
            ssl.Flush();

            string response = "";
            buffer = new byte[2048];
            bytes = ssl.Read(buffer, 0, 2048);
            //sb = new StringBuilder();
            //sb.Append(Encoding.ASCII.GetString(buffer));
            response = Encoding.ASCII.GetString(buffer);

            return response;
        }
        catch (Exception ex)
        {
            throw new ApplicationException(ex.Message);
        }
    }

    private bool deleteEmail(string sUID)
    {
        string googleResponse;
        try
        {
            tcpc = new System.Net.Sockets.TcpClient("imap.gmail.com", 993);
            ssl = new System.Net.Security.SslStream(tcpc.GetStream());
            ssl.AuthenticateAsClient("imap.gmail.com");
            receiveResponse("");
            //IMAP login command .  
            receiveResponse("$ LOGIN " + (String)Session["username"] + " " + (String)Session["password"] + "  \r\n");
            //Select the inbox folder
            receiveResponse("$ SELECT INBOX\r\n");

            googleResponse = receiveResponse("$ STORE "+ sUID + " +FLAGS (\\Deleted)\r\n");
            
            //GMAIL IMAP should be configured to expunge automatically. Otherwise, send expunge request to server.
            receiveResponse("$ LOGOUT\r\n");

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error deleting email(s): " + ex.Message);
        }
        finally
        {
            if (ssl != null)
            {
                ssl.Close();
                ssl.Dispose();
            }
            if (tcpc != null)
            {
                tcpc.Close();
            }
        }
        return false;
    }

    //oAuth get token logic. Uncomment if using oAuth.
    /* LEGACY
    private string getToken()
    {
        //Here we will send a post request to google and exchange our oAUTH code for a token, which will be used
        //to make all of the requests with the contact list during that particular session.
        HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(@"https://accounts.google.com/o/oauth2/token");
        ASCIIEncoding encoding = new ASCIIEncoding();
        string postData = "code=" + code;
        postData += "&client_id=<client_id_placeholder>";
        postData += "&client_secret=<client_secret_placeholder>";
        postData += "&redirect_uri=http://localhost:1172/messaging.aspx";
        postData += "&grant_type=authorization_code";
        byte[] data = encoding.GetBytes(postData);

        httpWReq.Method = "POST";
        httpWReq.ContentType = "application/x-www-form-urlencoded";
        httpWReq.Host = "accounts.google.com";
        httpWReq.ContentLength = data.Length;

        Stream stream = httpWReq.GetRequestStream();
        stream.Write(data, 0, data.Length);
        stream.Close();

        WebResponse response = httpWReq.GetResponse();
        Stream responseStream = response.GetResponseStream();
        StreamReader responseReader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
        string responseString = responseReader.ReadToEnd();
        JObject obj = JObject.Parse(responseString);
        string token = (string)obj.SelectToken("access_token");
        responseStream.Close();
        responseReader.Close();
        response.Close();
        return token;
    }*/

    protected void DeleteEmailButton_Click(object sender, EventArgs e)
    {
        List<TableRow> deleteList = new List<TableRow>();
        for(int i=0; i<InboxTable.Rows.Count; i++)
        {
            var cell = InboxTable.Rows[i].Cells[0];
            foreach (System.Web.UI.Control control in cell.Controls)
            {
                var checkBox = control as System.Web.UI.WebControls.CheckBox;
                if (checkBox != null)
                {
                    if (checkBox.Checked)
                    {
                        //load the row in the deleteList so we can delete those rows after deleting them from the imap server.
                        deleteList.Add(InboxTable.Rows[i]);
                        //The messages are listed in reverse order and we have to account for the header row.
                        int index = numEmails - i+2;
                        if (!deleteEmail(index.ToString()))
                        {
                            MessageBox.Show("Failed to Delete Message(s).");
                            break;
                        }
                    }
                }
            }
        }
        //Logout
        //delete the row(s) in the table.
        for(int i = 0; i<deleteList.Count; i++)
            InboxTable.Rows.Remove(deleteList[i]);
    }

    protected void ComposeButton_Click(object sender, EventArgs e)
    {
        if (ContactsListBox.SelectedIndex > -1)  //Make sure the user has made a selection
        {
            //Populate the To TextBox with the email of the contact the user has selected.
            int index = ContactsListBox.SelectedIndex;
            Contact con = _contacts[index];
            try
            {
                ToTextBox.Text = con.PrimaryEmail.Address;
            }
            catch { ToTextBox.Text = ""; }
        }
        //This shows the modal
        ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#compose').modal('toggle');</script>");
    }

    protected void DeleteContactButton_Click(object sender, EventArgs e)
    {
        if (ContactsListBox.SelectedIndex > -1)  //Make sure the user has made a selection
        {
            int index = ContactsListBox.SelectedIndex;
            Contact con = _contacts[index];
            RequestSettings settings = new RequestSettings("ProQuorum Messaging Module", (String)Session["username"], (String)Session["password"]);
            ContactsRequest cr = new ContactsRequest(settings);

            try
            {
                cr.Delete(con);
                _contacts.RemoveAt(index);      //Also remove the contacts from the lists so the ContactsListBox updates accordingly.
                _contactNames.RemoveAt(index);
                ContactsListBox.DataSource = null;      //update the listbox.
                ContactsListBox.DataSource = _contactNames;
                ContactsListBox.DataBind();
                title.Text = "Contact List (" + _contacts.Count.ToString() + " entries)";

                //MessageBox.Show(con.Name.FullName + " was successfully deleted from your contact list.");
                successOutput.Text = con.Name.FullName + " was successfully deleted from your contact list.";
                ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#successModal').modal('toggle');</script>");
            }
            catch (GDataVersionConflictException ex)
            {
                //MessageBox.Show("There was an Etag mismatch. Deletion could not be completed. Error: " + ex);
                errorOutput.Text = "There was an Etag mismatch. Deletion could not be completed. Error: " + ex;
                ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#errorModal').modal('toggle');</script>");
            }
        }
        else
        {
            errorOutput.Text = "Please select a contact from the list.";
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#errorModal').modal('toggle');</script>");
            //MessageBox.Show("Please select a contact from the list.");
        }
    }

    protected void sendEmailButton_Click(object sender, EventArgs e)
    {
        MailMessage m = new MailMessage((String)Session["username"], ToTextBox.Text.Trim());
        m.Subject = SubjectTextBox.Text.Trim(); ;
        m.Body = MessageTextBox.Text.Trim();

        SmtpClient smtpClient = new SmtpClient();
        smtpClient.EnableSsl = true;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential((String)Session["username"], (String)Session["password"]);
        smtpClient.Host = "smtp.gmail.com";
        try
        {
            smtpClient.Send(m);
            //Reset the text boxes to empty if the message was sent.
            ToTextBox.Text = "";
            SubjectTextBox.Text = "";
            MessageTextBox.Text = "";

            //MessageBox.Show("Message Sent.");
            successOutput.Text = "Message Sent.";
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#successModal').modal('toggle');</script>");
        }
        catch (Exception ex)
        {
            errorOutput.Text = "Your message was not sent. Error: " + ex.ToString();
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#errorModal').modal('toggle');</script>");
            //MessageBox.Show("Your message was not sent. Error: " + ex.ToString());
        }
    }

    protected void EditButton_Click(object sender, EventArgs e)
    {
        editContactFlag = true;
        if (ContactsListBox.SelectedIndex > -1)  //Make sure the user has made a selection
        {
            addSubmitButton.Text = "Update";
            int index = ContactsListBox.SelectedIndex;
            contact = _contacts[index];
            //set all of the text fields to the values in the contact
            try
            {
                fullName.Text = contact.Name.FullName;
            }
            catch { }
            try
            {
                notes.Text = contact.Content;
            }
            catch { }
            try
            {
                prEmail.Text = contact.PrimaryEmail.Address;
            }
            catch { }
            try
            {
                homePhone.Text = contact.PrimaryPhonenumber.Value;
            }
            catch { }
            try
            {
                address.Text = contact.PrimaryPostalAddress.Street;
            }
            catch { }
            try
            {
                city.Text = contact.PrimaryPostalAddress.City;
            }
            catch { }
            try
            {
                state.Text = contact.PrimaryPostalAddress.Region;
            }
            catch { }
            try
            {
                zip.Text = contact.PrimaryPostalAddress.Postcode;
            }
            catch { }
            try
            {
                country.Text = contact.PrimaryPostalAddress.Country;
            }
            catch { }
            //This shows the modal.
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#addContact').modal('toggle');</script>");
        }
        else
        {
            //MessageBox.Show("Please select a contact from the list.");
            errorOutput.Text = "Please select a contact from the list.";
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#errorModal').modal('toggle');</script>");
        }
    }

    protected void AddContactButton_Click(object sender, EventArgs e)
    {
        editContactFlag = false;
        //Reset all of the text boxes in case they are not empty.
        fullName.Text = "";
        notes.Text = "";
        prEmail.Text = "";
        homePhone.Text = "";
        address.Text = "";
        city.Text = "";
        state.Text = "";
        zip.Text = "";
        country.Text = "";
        //This shows the modal.
        ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#addContact').modal('toggle');</script>");
    }

    //This event controls the submit button for both the add contact modal and the edit contact modal.
    protected void addSubmitButton_Click(object sender, EventArgs e)
    {
        RequestSettings settings = new RequestSettings("ProQuorum Messaging Module", (String)Session["username"], (String)Session["password"]);
        ContactsRequest cr = new ContactsRequest(settings);
        if (!editContactFlag)   //If we are adding a contact and not editing one.
        {
            Contact newEntry = new Contact();
            // Set the contact's name.
            if (fullName.Text != "")    //make sure the user has put in a full name.
            {
                newEntry.Name = new Name()
                {
                    FullName = fullName.Text
                    //GivenName = "Elizabeth",
                    //FamilyName = "Bennet",
                };
            }
            if (notes.Text != "")
                newEntry.Content = notes.Text;

            // Set the contact's e-mail addresses.
            if (prEmail.Text != "")         //make sure the user has put in a primary email.
            {
                newEntry.Emails.Add(new EMail()
                {
                    Primary = true,
                    Rel = ContactsRelationships.IsHome,
                    Address = prEmail.Text
                });
            }
            // Set the contact's phone numbers.
            if (homePhone.Text != "")
            {
                newEntry.Phonenumbers.Add(new PhoneNumber()
                {
                    Primary = true,
                    Rel = ContactsRelationships.IsHome,
                    Value = homePhone.Text
                });
            }
            /* Set the contact's IM information.
            newEntry.IMs.Add(new IMAddress()
            {
                Primary = true,
                Rel = ContactsRelationships.IsHome,
                Protocol = ContactsProtocols.IsGoogleTalk,
            });*/
            // Set the contact's postal address.
            if (address.Text != "" || city.Text != "" || state.Text != "" || zip.Text != "" || country.Text != "")
            {
                newEntry.PostalAddresses.Add(new StructuredPostalAddress()
                {
                    Rel = ContactsRelationships.IsWork,
                    Primary = true,
                    Street = address.Text,
                    City = city.Text,
                    Region = state.Text,
                    Postcode = zip.Text,
                    Country = country.Text
                    //FormattedAddress = "1600 Amphitheatre Pkwy Mountain View",
                });
            }
            // Insert the contact.
            Uri feedUri = new Uri(ContactsQuery.CreateContactsUri("default"));
            Contact createdEntry = cr.Insert(feedUri, newEntry);

            //Update the contacts list box to reflect the new contact as well as the contacts data structures.
            _contacts.Add(newEntry);
            _contactNames.Add(newEntry.Name.FullName);
            //Sort both the lists of contacts in alphabetical order
            _contactNames.Sort();
            _contacts = _contacts.OrderBy(o => o.Name.FullName).ToList();
            ContactsListBox.DataSource = null;      //update the listbox.
            ContactsListBox.DataSource = _contactNames;
            ContactsListBox.DataBind();
            title.Text = "Contact List (" + _contacts.Count.ToString() + " entries)";

            //MessageBox.Show("Contact was successfully added.");
            successOutput.Text = "Contact was successfully added.";
            ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#successModal').modal('toggle');</script>");
        }
        else //We are editing a contact.
        {
            try
            {
                //set all of the contacts fields to the new values of the text fields.
                if (fullName.Text.Length > 0)
                    contact.Name.FullName = fullName.Text;
                else
                    contact.Name.FullName = "";

                if (notes.Text.Length > 0)
                    contact.Content = notes.Text;
                else
                    contact.Content = "";

                if (prEmail.Text.Length > 0)
                    contact.PrimaryEmail.Address = prEmail.Text;
                else
                    contact.PrimaryEmail.Address = "";

                if (homePhone.Text.Length > 0)
                    contact.PrimaryPhonenumber.Value = homePhone.Text;
                else
                    contact.PrimaryPhonenumber.Value = "";

                if (address.Text.Length > 0)
                    contact.PrimaryPostalAddress.Street = address.Text;
                else
                    contact.PrimaryPostalAddress.Street = "";

                if (city.Text.Length > 0)
                    contact.PrimaryPostalAddress.City = city.Text;
                else
                    contact.PrimaryPostalAddress.City = "";

                if (state.Text.Length > 0)
                    contact.PrimaryPostalAddress.Region = state.Text;
                else
                    contact.PrimaryPostalAddress.Region = "";

                if (zip.Text.Length > 0)
                    contact.PrimaryPostalAddress.Postcode = zip.Text;
                else
                    contact.PrimaryPostalAddress.Postcode = "";

                if (country.Text.Length > 0)
                    contact.PrimaryPostalAddress.Country = country.Text;
                else
                    contact.PrimaryPostalAddress.Country = "";

                Contact updatedContact = cr.Update(contact);

                //MessageBox.Show("Contact was updated successfully.");
                successOutput.Text = "Contact was updated successfully.";
                ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#successModal').modal('toggle');</script>");
            }
            catch (GDataVersionConflictException ex)
            {
                //MessageBox.Show("Etag mismatch. Could not update contact.");
                errorOutput.Text = "Etag mismatch. Could not update contact.";
                ClientScript.RegisterStartupScript(GetType(), "Show", "<script> $('#errorModal').modal('toggle');</script>");
            }
        }
    }
}