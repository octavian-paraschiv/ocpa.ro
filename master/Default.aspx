<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="master.Default" %>

<asp:Content ID="headContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">

<h2 style="font-weight: bold;">Welcome to OcPa's web site! Please choose one of the categories below:</h2>
    
<table>
        <tr>
            <td>
                <div class="container">
                    <a href="/meteo">
                        <img src="images/clouds.jpg" style="height: 200px; width: 400px"/>
                        <div class="bottom-centered">Weather Forecast</div>
                    </a>
                </div>
            </td>
            <td>
                <div class="container">
                    <a href="/protone">
                        <img src="images/protone.png" style="height: 200px; width: 400px"/>
                        <div class="bottom-centered" style="color: black;">ProTONE Player</div>
                    </a>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="container">
                    <a href="/electronics.aspx">
                        <img src="images/electronics.jpg" style="height: 200px; width: 400px"/>
                        <div class="bottom-centered">Electronics</div>
                    </a>
                </div>
            </td>
             <td>
                <div class="container">
                    <a href="./photography.aspx">
                        <img src="images/Photography.jpg" style="height: 200px; width: 400px"/>
                        <div class="bottom-centered">Photography</div>
                    </a>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <div class="container">
                    <a href="/programming">
                        <img src="images/programming.jpg" style="height: 200px; width: 805px"/>
                        <div class="bottom-centered">Other programming stuff</div>
                    </a>
                </div>
            </td>
        </tr>
</table>


</asp:Content>
