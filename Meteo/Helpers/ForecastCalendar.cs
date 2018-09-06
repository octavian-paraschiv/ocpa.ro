using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Meteo.Helpers
{
    public class ForecastCalendar
    {
        HtmlTable _tblCalendar = null;
        string _viewport = "Romania";

        public ForecastCalendar(HtmlTable tblCalendar, string viewport)
        {
            _tblCalendar = tblCalendar;
            _viewport = viewport;
        }

        public void BuildCalendar(int r, int c)
        {
            DateTime today = DateTime.Today;
            DateTime dtStart = today;
            DateTime dtEnd = today;

            int days = 0;

            int totalDays = DataHelper.GetCalendarRange(ref dtStart, ref dtEnd, days);
            int weeks = (int)Math.Floor(totalDays / 7f);

            _tblCalendar.Rows.Clear();

            DateTime dt = dtStart;
            var startDayFound = false;

            var colder = ScaleSettings.Temperature.Colder;
            var cold = ScaleSettings.Temperature.Cold;
            var warm = ScaleSettings.Temperature.Warm;
            var warmer = ScaleSettings.Temperature.Warmer;
            var hot = ScaleSettings.Temperature.Hot;
            var frost = ScaleSettings.Temperature.Frost;

            for (int w = 0; w <= weeks; w++)
            {
                var tr = new HtmlTableRow();

                for (int d = 0; d < 7; d++)
                {
                    var td = new HtmlTableCell();

                    td.Attributes.Add("style", "vertical-align: top; text-align: justify;");

                    StringBuilder sb = new StringBuilder();

                    if (!startDayFound)
                    {
                        if ((int)dt.DayOfWeek == d)
                        {
                            startDayFound = true;
                        }
                    }

                    if (startDayFound && dt <= dtEnd)
                    {
                        int tMin = (int)Math.Round(DataHelper.GetDataPoint("T_SL", dt, r, c));
                        int tMax = (int)Math.Round(DataHelper.GetDataPoint("T_SH", dt, r, c));

                        string cellColor = "#EFEFEF";

                        float lat = 0;

                        switch (_viewport)
                        {
                            case "Europe":
                                lat = 89f - r;
                                break;

                            case "Romania":
                            default:
                                lat = 49f - (float)r / 2;
                                break;
                        }

                        List<string> risks = new List<string>();

                        float snow = PrecipHelper.GetSnowThickness(dt, r, c);

                        float refMin = 0, refMax = 0;

                        RefTemp.GetRefTemps_ByLatitude(dt.DayOfYear, lat, ref refMin, ref refMax);

                        int tRefMin = (int)Math.Round(refMin);
                        int tRefMax = (int)Math.Round(refMax);

                        bool bgSet = false;

                        string tempType = "";

                        if (!bgSet && tMax >= hot)
                        {
                            // heat wave
                            cellColor = "lightpink";
                            risks.Add("Extreme heat");
                            tempType = "Extreme heat";
                            bgSet = true;
                        }

                        if (!bgSet && (tMin <= frost || tMax <= frost))
                        {
                            // Frosty
                            cellColor = "lightblue";
                            risks.Add("Deep freeze");
                            tempType = "Bitterly cold";
                            bgSet = true;
                        }

                        if (!bgSet && tMax > (tRefMax + warmer))
                        {
                            // Way warmer than normal
                            cellColor = "#FFCCAA";
                            tempType = "Much warmer than normal";
                            bgSet = true;
                        }

                        if (!bgSet && tMax > (tRefMax + warm))
                        {
                            // A little warmer than normal
                            cellColor = "#FFFF99";
                            tempType = "A little warmer than normal";
                            bgSet = true;
                        }

                        if (!bgSet && tMax < (tRefMax + colder))
                        {
                            // Way colder than normal
                            cellColor = "powderblue";
                            tempType = "Much colder than normal";
                            bgSet = true;
                        }

                        if (!bgSet && tMax < (tRefMax + cold))
                        {
                            // Colder than normal
                            cellColor = "lightcyan";
                            tempType = "A little colder than normal";
                            bgSet = true;
                        }

                        string precip = PrecipHelper.GetWeatherType(dt, r, c, risks);

                        string borderColor = "darkgray";
                        int borderSize = 1;

                        string dtStr = dt.ToString("ddd, dd-MMM-yyyy");
                        string toolTip = StringHelper.GetToolTip(dtStr, precip, tempType);

                        if (dt == today)
                        {
                            borderColor = "teal";
                            borderSize = 3;
                        }
                        else if (risks.Count > 0)
                        {
                            borderColor = "red";
                            borderSize = 2;
                        }

                        sb.AppendLine($"<table cellpadding=0 cellspacing=1 width='100%' height='100%'>");

                        // Date label
                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td colspan='2' style='background-color: darkgray;'>");
                        sb.AppendLine($"<div class='dateLabel' style='color: black;'>{dtStr}</div");
                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>"); // Date label

                        sb.AppendLine("<tr>");

                        sb.AppendLine("<td>");
                        sb.AppendLine($"<img src='Images/precip_{precip}.png' style='width: 48px; height: 48px; margin-top: 2px;'/>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("<td style='vertical-align: middle;'>");

                        // Temperatures table
                        sb.AppendLine("<table cellpadding='0' cellspacing='2'>");

                        // day
                        sb.AppendLine("<tr>");

                        sb.AppendLine("<td>");
                        sb.Append("<img src='Images/day.png'/>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("<td>");
                        sb.Append($"<div class='actualTempLabel' style='color: red;'>{Scale(tMax)}</div>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("<td>");
                        sb.Append($"<div class='refTempLabel' style='color: red;'>[{Scale(tRefMax)}]</div>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("</tr>"); // day

                        // night
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td>");
                        sb.Append("<img src='Images/night.png'/>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("<td>");
                        sb.Append($"<div class='actualTempLabel' style='color: blue;'>{Scale(tMin)}</div>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("<td>");
                        sb.Append($"<div class='refTempLabel' style='color: blue;'>[{Scale(tRefMin)}]</div>");
                        sb.AppendLine("</td>");

                        sb.AppendLine("</tr>"); // night

                        sb.AppendLine("</table>"); // Temperatures table


                        // Snow layer and risks info if any ...
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td colspan='2'>");
                        sb.AppendLine("<table cellpadding=0 cellspacing=0 width='100%'>");

                        // Snow layer if any ...
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td>");

                        if (snow >= 300)
                            sb.AppendLine($"<div class='extraLabel' style='color: teal;'>Snow cover: >300 cm</div>");
                        else if (snow > 0)
                            sb.AppendLine($"<div class='extraLabel' style='color: teal;'>Snow cover: {snow} cm</div>");

                        // Risks if any ...
                        if (risks.Count > 0)
                            sb.AppendLine($"<div class='extraLabel' style='color: red;'>{EnumRisks(risks)}</div>");

                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>");

                        sb.AppendLine("</table>");

                        sb.AppendLine("<td>");
                        sb.AppendLine("</tr>");

                        sb.AppendLine("</table>");

                        td.BgColor = cellColor;
                        td.InnerHtml = sb.ToString();

                        dt = dt.AddDays(1);

                        td.Attributes.Add("style", $"border: {borderSize}px solid {borderColor};");
                        td.Attributes.Add("title", toolTip);
                    }
                    else
                    {
                        DateTime vdt = dt.AddDays(d - 6);

                        string dtStr = vdt.ToString("ddd, dd-MMM-yyyy");

                        sb.AppendLine($"<table cellpadding=0 cellspacing=1 width='100%' height='100%'>");

                        // Date label
                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td colspan='2' style='background-color: darkgray; color: lightgray;'>");
                        sb.AppendLine($"<div class='dateLabel' style='color: black;'>{dtStr}</div");
                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>"); // Date label

                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td>");
                        sb.AppendLine("No data available ... ");
                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>");

                        sb.AppendLine("</table>");

                        td.InnerHtml = sb.ToString();

                        td.Attributes.Add("style", "background-color: lightgray; color: black; ");
                        td.Attributes.Add("title", "No data available");
                    }

                    tr.Cells.Add(td);
                }

                _tblCalendar.Rows.Add(tr);
            }
        }

        private string EnumRisks(List<string> risks)
        {
            StringBuilder sb = new StringBuilder();

            if (risks != null)
            {
                for (int i = 0; i < risks.Count; i++)
                    sb.AppendLine($"<div class='extraLabel' style='color: red;'><img src='Images/warning.png'>{risks[i]}</div>");
            }

            return sb.ToString();
        }

        private string Scale(int temp)
        {
            bool useC = true;

            if (useC)
                return $"{temp}&#x2103;";

            return $"{(int)Math.Round(9 * (temp + 32f) / 5)}&#x2109;";
        }
    }
}