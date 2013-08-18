<%@ Page Title="Messaging System" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="Contacts">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %></h1>           
            </hgroup>
        </div>
    </section>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h3>Welcome!</h3>
   <p>Welcome to the messaging system. To get started, please login with your Google information below.</p>

       <!-- <asp:HyperLink class="btn" ID="HyperLink1" runat="server" NavigateUrl="https://accounts.google.com/o/oauth2/auth?scope=https://www.google.com/m8/feeds&amp;response_type=code&amp;redirect_uri=http://localhost:1172/messaging.aspx&amp;client_id=255810871926.apps.googleusercontent.com">Login</asp:HyperLink> -->
       <div style="text-align: center; width:100%;">
         <table style="margin: auto;">
            <tr>
                <td style="text-align: right;" ><b>Gmail:</b></td>
                <td style="text-align: right;"><asp:TextBox ID="userName" runat="server" TextMode="Email" style="z-index: 1; width: 250px;" TabIndex="1"></asp:TextBox></td>
            </tr>
            <tr>
                <td style="text-align: right;"><b>Password:</b></td>
                <td style="text-align: right;"><asp:TextBox ID="password" runat="server" style="z-index: 1; width: 250px;" TextMode="Password" TabIndex="2"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2" style="text-align: right;">
                <asp:Button class="btn" ID="loginButton" runat="server" OnClick="loginButton_Click" style="z-index: 1; width: 98px; height: 33px;" Text="Submit" TabIndex="3"/>
                <p><asp:Label ID="Output" RunAt="server" ForeColor="Red" /></p>
                </td>
            </tr>
          </table>
        </div>
</asp:Content>