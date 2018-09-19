<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProTONE.Default" %>

<asp:Content ID="headContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div runat="server" id="hint">
        <h3 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;ProTONE Player - released versions:</h3>
    </div>
    <table runat="server" id="tblProtoneVersions" cellpadding="0" cellspacing="5">
            <!-- DON'T ADD ANY ELEMENT HERE -->
    </table>

</asp:Content>
