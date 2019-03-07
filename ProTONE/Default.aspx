<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProTONE.Default" %>

<asp:Content ID="headContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">
     <div runat="server" id="Div3">
        <h3 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;The recommended way to install ProTONE is to download and run the Installer.</h3>
    </div>
    <br />
    <div runat="server" id="Div1">
        <h3 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;Choose the latest ProTONE release::</h3>
    </div>
    <table runat="server" id="tblProtoneCurrent" cellpadding="0" cellspacing="5">
            <!-- DON'T ADD ANY ELEMENT HERE -->
    </table>
    <br />
    <div runat="server" id="hint">
        <h3 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;Or choose an older ProTONE release that you already used and you liked it (although it will not have the latest features and bugfixes):</h3>
    </div>
    <table runat="server" id="tblProtoneVersions" cellpadding="0" cellspacing="5">
            <!-- DON'T ADD ANY ELEMENT HERE -->
    </table>
    <br />
     <div runat="server" id="Div2">
        <h4 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;And if you feel lucky, choose an experimental ProTONE version. You could benefit of some new added features, but yet not tested - so use it at your own risk:</h4>
    </div>
    <table runat="server" id="tblExperimental" cellpadding="0" cellspacing="5">
            <!-- DON'T ADD ANY ELEMENT HERE -->
    </table>
    <br />
    <h4 style='margin: 0px; padding: 10px 0px 10px 0px;'>&nbsp;If you are a programmer, you might also want to download or clone the ProTONE code from its <a target="new" href="https://github.com/octavian-paraschiv/protone-suite">ProTONE Github Repository</a>.</h4>
    <h4 style='margin: 0px; padding: 0px 0px 10px 0px;'>&nbsp;Visual Studio 2017 with .NET Development workload and .NET 4.0 targeting pack is required to build the code.</h4>
</asp:Content>
