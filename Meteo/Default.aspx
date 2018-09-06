<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Meteo.Default" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<table cellpadding="0" cellspacing="0" style="width: 100%;">
    <tr>
        <td colspan="3">
            <h3>User's guide for this site:</h3>
            <ol class="list">
                <li>Select the area for the forecast by making the proper selection in the corresponding drop down list.</li>
                <li>In the map below, click on a grid cell that roughly corresponds to the city or region to include in your forecast.</li>
                <li>The selected cell will be surrounded by a red rectangle.</li>
                <li>The right pane will show a list with the forecasted temperatures and phenomena for the available date range.</li>
            </ol>
        </td>
    </tr>

    <tr>
        <td colspan="3">
            <h3 class="dropDown">Geographical area for the forecast:
            <asp:DropDownList CssClass="dropDown" runat="server" ID="cmbViewport" AutoPostBack="true" OnSelectedIndexChanged="cmbViewport_SelectedIndexChanged">
                <asp:ListItem Selected="True">Romania</asp:ListItem>
                <asp:ListItem>Europe</asp:ListItem>
            </asp:DropDownList>
            </h3>
        </td>
    </tr>

    <tr>
        <td colspan="3">
            &nbsp;
        </td>
    </tr>

    <tr>
        <td>
             <input type="image" src="~/Images/Ro_grid.PNG" ID="ImgButton" runat="server" OnServerClick="ImgButton_ServerClick" 
                    style="width: 600px; height: 450px; border: 1px solid lightgray;"  />
        </td>
        <td>
            &nbsp;
        </td>
        <td rowspan="2" runat="server" id="borderCell" style="border: 1px solid lightgray">
            <div runat="server" id="hint">
                <h3>&nbsp;Please click on the map to select a region for your forecast ...</h3>
            </div>
            <table runat="server" id="tblCalendar" cellpadding="0" cellspacing="5">
                <!-- DON'T ADD ANY ELEMENT HERE -->
            </table>
        </td>
    </tr>
</table>
</asp:Content>
