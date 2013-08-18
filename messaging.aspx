<%@ Page Title="Inbox" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="messaging.aspx.cs" Inherits="Default2" Debug="true" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="Contacts">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %></h1>           
            </hgroup>
        </div>
    </section>
    <div id="AddressListBox" style="text-align: center; width:100%;">
        <table>
            <tr>
                <td><b><asp:Label ID="title" runat="server" Text="Contact List"></asp:Label>:</b>
                </td>
            </tr>
            <tr>
                <td><asp:ListBox ID="ContactsListBox" runat="server" Height="418px" Width="300px" style="position: ;"></asp:ListBox></td>
            </tr>
            <tr>
                <td>
                    <%-- <a href="#addContact" role="button" class="btn" data-toggle="modal">Add</a> --%>
                    <asp:Button class="btn" ID="AddContactButton" runat="server" OnClick="AddContactButton_Click" Text="Add" Width="90px" />
                    <asp:Button class="btn" ID="DeleteContactButton" runat="server" OnClick="DeleteContactButton_Click" Text="Delete" Width="90px" />
                    <asp:Button class="btn" ID="EditContactButton" runat="server" OnClick="EditButton_Click" Text="Edit" Width="90px" />

                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:Table ID="InboxTable" runat="server" CellPadding="3">
        <asp:TableHeaderRow ID="toolbar" runat="server" BackColor="#7ac0da" Height="10">
            <asp:TableHeaderCell Scope="Column" ColumnSpan="4">
                <div class="btn-group">
                <asp:ImageButton class="btn" ID="Button1" runat="server" ImageUrl="~/Images/compose.png" Width="40" Height="40" OnClick="ComposeButton_Click" Text="Compose" BorderStyle="Solid" BorderColor="Black"/>
                <asp:ImageButton class="btn" ID="Button2" runat="server" ImageUrl="~/Images/trash.png" Width="40" Height="40" OnClick="DeleteEmailButton_Click" Text="Delete" BorderStyle="Solid" BorderColor="Black" />
                </div>
            </asp:TableHeaderCell>
        </asp:TableHeaderRow>
        <asp:TableHeaderRow ID="emailHeaderRow" runat="server" BackColor="#e9e9e9">
            <asp:TableHeaderCell 
                Scope="Column"
                Text="" Width="50px" />
            <asp:TableHeaderCell 
                Scope="Column" 
                Text="From" />
            <asp:TableHeaderCell  
                Scope="Column" 
                Text="Subject" />
            <asp:TableHeaderCell 
                Scope="Column" 
                Text="Date" />
        </asp:TableHeaderRow>
    </asp:Table>

    <%-- Compose Modal Box--%>
    <div id="compose" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Compose E-Mail</h3>
        </div>
        <div class="modal-body">
            <b>To:</b><br />
            <asp:TextBox ID="ToTextBox" runat="server" style="z-index: 1; width: 600px;" TabIndex="1"></asp:TextBox><br />
            <b>Subject:</b><br />
            <asp:TextBox ID="SubjectTextBox" runat="server" style="z-index: 1; width: 600px;" TabIndex="2"></asp:TextBox><br />
            <b>Message:</b><br />              
            <asp:TextBox ID="MessageTextBox" runat="server" style="z-index: 1; width: 600px; height:300px;" TabIndex="3" TextMode="MultiLine"></asp:TextBox><br />               
        </div>
        <div class="modal-footer">
            <asp:Button class="btn btn-primary" ID="sendEmailButton" runat="server" OnClick="sendEmailButton_Click" Text="Send" Width="90px" />
        </div>
    </div>

    <%-- Add/Edit Contact Modal Box --%>
    <div id="addContact" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel2" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Add Contact</h3>
        </div>
        <div class="modal-body">
            <table style="width:550px;">
                <tr>
                    <td><b>Full Name:</b></td>
                    <td><b>Primary Email:</b></td>
                </tr>
                <tr>
                    <td><asp:TextBox ID="fullName" runat="server" style="z-index: 1; width: 275px;" TabIndex="1"></asp:TextBox></td>                    
                    <td><asp:TextBox ID="prEmail" runat="server" style="z-index: 1; width: 275px;" TextMode="Email" TabIndex="2"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><b>Phone:</b></td>
                    <td><b>Address:</b></td>
                </tr>
                <tr>                    
                    <td><asp:TextBox ID="homePhone" runat="server" style="z-index: 1; width: 275px" TextMode="Phone" TabIndex="4"></asp:TextBox></td>
                    <td><asp:TextBox ID="address" runat="server" style="z-index: 1; width: 275px" TabIndex="6"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><b>City:</b></td>
                    <td><b>State:</b></td>
                </tr>
                <tr>
                    <td><asp:TextBox ID="city" runat="server" style="z-index: 1; width: 275px" TabIndex="7"></asp:TextBox></td>
                    <td><asp:TextBox ID="state" runat="server" style="z-index: 1; width: 275px" TabIndex="8"></asp:TextBox></td> 
                </tr>
                <tr>
                    <td><b>ZIP:</b></td>
                    <td><b>Country:</b></td>
                </tr>
                <tr>                    
                    <td><asp:TextBox ID="zip" runat="server" style="z-index: 1; width: 275px" MaxLength="5" TabIndex="9"></asp:TextBox></td>                      
                    <td><asp:TextBox ID="country" runat="server" style="z-index: 1; width: 275px" TabIndex="10"></asp:TextBox></td>            
                </tr>
                <tr>
                    <td colspan="2"><b>Comments:</b></td>
                </tr>
                <tr>
                    <td colspan="2"><asp:TextBox ID="notes" runat="server" style="z-index: 1; height: 132px; width: 550px" TextMode="MultiLine" TabIndex="11"></asp:TextBox></td>
                </tr>
            </table>
        </div>
        <div class="modal-footer">
            <asp:Button class="btn" ID="addSubmitButton" runat="server" OnClick="addSubmitButton_Click" style="z-index: 1; width: 98px; height: 33px;" Text="Submit" TabIndex="12"/>
        </div>
    </div>

     <div id="viewMessage" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabe3" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3><asp:Label ID="subjectLabel" runat="server" style="z-index:1;" /></h3>
        </div>
        <div class="modal-body">
            <b>Message:</b><br />              
            <asp:Label ID="ViewMessageTextBox" runat="server" style="z-index: 1; width: 600px; height:300px;" TabIndex="3" TextMode="MultiLine" /><br />               
        </div>
        <div class="modal-footer">
            <%-- Buttons can go in here --%>
        </div>
    </div>

    <%-- Error Modal Box--%>
    <div id="errorModal" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel4" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Something Went Wrong!</h3>
        </div>
        <div class="modal-body">
            <p><asp:Label ID="errorOutput" RunAt="server" ForeColor="Red" /></p>
        </div>
        <div class="modal-footer">
            
        </div>
    </div>
    <%-- Success Modal Box--%>
    <div id="successModal" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel4" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Success!</h3>
        </div>
        <div class="modal-body">
            <p><asp:Label ID="successOutput" RunAt="server" /></p>
        </div>
        <div class="modal-footer">
            
        </div>
    </div>

    <script type="text/javascript">
        //Link Function.
        function link(tableRow, flag) { //the flag defines whether the user has unchecked the checkbox or not.
            var rowNumber = tableRow.rowIndex;
            var messageText = tableRow.cells[4].innerHTML;
            var subjectText = tableRow.cells[2].innerHTML;
            var messageLabel = document.getElementById('<%=ViewMessageTextBox.ClientID%>');
            var subjectLabel = document.getElementById('<%=subjectLabel.ClientID%>');
            subjectLabel.innerHTML = subjectText;
            messageLabel.innerHTML = messageText;

            rowCheck = tableRow.cells[0].getElementsByTagName('input')[0].checked == true;
            
            //if the user did not have interaction with the checkbox, show the modal.
            //Otherwise, don't show the modal and reset the flag to false.
            if (!flag)
                $("#viewMessage").modal('show');
            else {
                tableRow.removeAttribute("onclick");
                tableRow.setAttribute("onclick", "link(this, false);");
            }
        }
    </script>
</asp:Content>