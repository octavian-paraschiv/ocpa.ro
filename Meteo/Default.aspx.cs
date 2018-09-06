using System;
using System.Collections.Generic;
using System.Web.UI;
using System.IO;
using Meteo.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Text;
using System.Web.UI.HtmlControls;
using Meteo.Helpers;

namespace Meteo
{
    public partial class Default : System.Web.UI.Page
    {
        string _viewport = "Romania";
        GriddedMap _grid = null;
        ForecastCalendar _calendar = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                    SetViewport();
            }
            catch (Exception ex)
            {
                base.Response.Redirect("~/Default.aspx");
            }
        }

        protected void ImgButton_ServerClick(object sender, ImageClickEventArgs e)
        {
            try
            {
                _grid = new GriddedMap(_viewport);

                int c = _grid.Cols * e.X / 600;
                int r = _grid.Rows * e.Y / 450;

                DrawRegionGrid(r, c);
                DisplayData(r, c);
            }
            catch (Exception ex)
            {
                base.Response.Redirect("~/Default.aspx");
            }
        }

        protected void cmbViewport_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewState["viewport"] = cmbViewport.SelectedValue;
            SetViewport();
        }

        private void SetViewport()
        {
            _viewport = (ViewState["viewport"] as string) ?? "Romania";

            _grid = new GriddedMap(_viewport);

            AppFolders.Rebuild(Request, _viewport);

            DrawRegionGrid(-1, -1);

            borderCell.Attributes["style"] = "width: 100%; border: 1px solid lightgray";
            hint.Visible = true;
        }

        private void DisplayData(int r, int c)
        {
            try
            {
                _calendar = new ForecastCalendar(tblCalendar, _viewport);
                _calendar.BuildCalendar(r, c);

                borderCell.Attributes["style"] = "width: 100%";

                hint.Visible = false;
            }
            catch (Exception ex)
            {
                Response.Redirect("~/Default.aspx");
                Response.Write(ex.Message);
            }
        }

        private void DrawRegionGrid(int r, int c)
        {
            string tempFile = _grid.DrawHighlightedArea(r, c);
            if (string.IsNullOrEmpty(tempFile))
                ImgButton.Src = $"~/Images/{_viewport}_grid.png";
            else
                ImgButton.Src = string.Format("~/genImages/{0}", tempFile);
        }
    }
}