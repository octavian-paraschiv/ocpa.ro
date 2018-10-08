using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using ThorusCommon.IO;

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

            //--------
            // Table header row
            var headerRow = new HtmlTableRow();
            for (int d = 0; d <= 7; d++)
            {
                HtmlTableCell cell = new HtmlTableCell();
                headerRow.Cells.Add(cell);

                string dayBackColor = "black";
                string dayTextColor = "white";

                StringBuilder sb = new StringBuilder();
                if (d == 0)
                {
                    sb.AppendLine($"<div class='dateLabelSummary' style='background-color: black; color: white;'>Week Summary</div");
                    HtmlTableCell sepCell = new HtmlTableCell();
                    sepCell.Attributes.Add("class", "collapsible");
                    sepCell.Attributes.Add("style", "width: 10px");
                    headerRow.Cells.Add(sepCell);

                    cell.Attributes.Add("style", $"'width: 400px; min-width: 400px; background-color: {dayBackColor}; color: {dayTextColor};");
                }
                else
                {
                    DayOfWeek dw = (DayOfWeek)(d % 7);

                    switch (dw)
                    {
                        case DayOfWeek.Saturday:
                            dayBackColor = "maroon";
                            dayTextColor = "white";
                            break;
                        case DayOfWeek.Sunday:
                            dayBackColor = "maroon";
                            dayTextColor = "white";
                            break;
                        default:
                            break;
                    }

                    sb.AppendLine($"<div class='dateLabelSummary' style='background-color: {dayBackColor}; color: {dayTextColor};'>{dw}</div");
                    cell.Attributes.Add("class", "collapsible");

                    cell.Attributes.Add("style", $"'background-color: {dayBackColor}; color: {dayTextColor};");
                }
                
                cell.InnerHtml = sb.ToString();
            }
            _tblCalendar.Rows.Add(headerRow);

            HtmlTableRow sep = new HtmlTableRow();
            sep.Attributes.Add("style", "height: 3px;");
            _tblCalendar.Rows.Add(sep);
            //--------

            //--------
            for (int w = 0; w <= weeks; w++)
            {
                var tr = new HtmlTableRow();

                WeekStats stats_tMin = new WeekStats();
                WeekStats stats_tMax = new WeekStats();
                WeekStats stats_tRefMin = new WeekStats();
                WeekStats stats_tRefMax = new WeekStats();
                WeekStats stats_snow = new WeekStats();

                Dictionary<string, int> phenomena = new Dictionary<string, int>();

                string dayBackColor = "darkgray";
                string dayTextColor = "black";

                DateTime? weekStart = null;
                DateTime? weekEnd = null;

                HtmlTableCell weekSummaryCell = new HtmlTableCell();

                for (int d = 0; d <= 7; d++)
                {
                    if (d == 0)
                    {
                        #region Week summary cell + separator
                        tr.Cells.Add(weekSummaryCell);
                        HtmlTableCell sepCell = new HtmlTableCell();
                        sepCell.Attributes.Add("class", "collapsible");
                        sepCell.Attributes.Add("style", "width: 10px");
                        tr.Cells.Add(sepCell);
                        #endregion
                    }
                    else
                    {
                        #region Week day cell

                        HtmlTableCell td = new HtmlTableCell();

                        StringBuilder sb = new StringBuilder();

                        if (!startDayFound)
                        {
                            if ((int)dt.DayOfWeek == (d))
                            {
                                startDayFound = true;
                            }
                        }

                        if (startDayFound && dt <= dtEnd)
                        {
                            int tMin = (int)Math.Round(DataHelper.GetDataPoint("T_SL", dt, r, c));
                            int tMax = (int)Math.Round(DataHelper.GetDataPoint("T_SH", dt, r, c));
                            int tRefMin = (int)Math.Round(DataHelper.GetDataPoint("T_NL", dt, r, c));
                            int tRefMax = (int)Math.Round(DataHelper.GetDataPoint("T_NH", dt, r, c));
                            int snow = PrecipHelper.GetSnowThickness(dt, r, c);

                            stats_tMin.Add(tMin);
                            stats_tMax.Add(tMax);
                            stats_tRefMax.Add(tRefMax);
                            stats_tRefMin.Add(tRefMin);
                            stats_snow.Add(snow);
                            
                            string cellColor = "#EFEFEF";

                            List<string> risks = new List<string>();

                            bool bgSet = false;

                            string tempType = "";

                            if (!bgSet && tMax >= hot)
                            {
                                // heat wave
                                cellColor = "lightpink";
                                risks.Add("Heat wave");
                                tempType = "Heat wave";
                                bgSet = true;
                            }

                            if (!bgSet && (tMin <= frost || tMax <= frost))
                            {
                                // Frosty
                                cellColor = "lightblue";
                                risks.Add("Frosty");
                                tempType = "Frosty";
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

                            string dtStr = dt.ToString("dd-MMMM-yyyy");

                            string toolTip = StringHelper.GetToolTip(dtStr, precip, tempType);

                            if (precip.Length >= 4)
                                phenomena.AddPhenomenon(precip.Substring(3), int.Parse(precip.Substring(0, 2)));
                            else
                                phenomena.AddPhenomenon("Sun", 0);

                            if (dt == today)
                            {
                                borderColor = "teal";
                                borderSize = 5;
                            }
                            //else if (risks.Count > 0)
                            //{
                            //    borderColor = "red";
                            //    borderSize = 2;
                            //}

                            // Cell table
                            sb.AppendLine($"<div style='vertical-align: middle;'>");
                            sb.AppendLine($"<table cellpadding=0 cellspacing=1 width='100%' height='100%'>"); 

                            // Date label
                            sb.AppendLine("<tr>");

                            switch (dt.DayOfWeek)
                            {
                                case DayOfWeek.Saturday:
                                    dayBackColor = "maroon";
                                    dayTextColor = "white";
                                    break;
                                case DayOfWeek.Sunday:
                                    dayBackColor = "maroon";
                                    dayTextColor = "white";
                                    break;
                                default:
                                    break;
                            }

                            sb.AppendLine($"<td colspan='2' style='background-color: {dayBackColor};'>");

                            sb.AppendLine($"<div class='dateLabel' style='color: {dayTextColor};'>{dtStr}</div");
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
                            sb.Append($"<div class='actualTempLabel' style='color: red;'>{StringHelper.Scale(tMax)}</div>");
                            sb.AppendLine("</td>");

                            sb.AppendLine("<td>");
                            sb.Append($"<div class='refTempLabel' style='color: red;'>[{StringHelper.Scale(tRefMax)}]</div>");
                            sb.AppendLine("</td>");

                            sb.AppendLine("</tr>"); // day

                            // night
                            sb.AppendLine("<tr>");
                            sb.AppendLine("<td>");
                            sb.Append("<img src='Images/night.png'/>");
                            sb.AppendLine("</td>");

                            sb.AppendLine("<td>");
                            sb.Append($"<div class='actualTempLabel' style='color: blue;'>{StringHelper.Scale(tMin)}</div>");
                            sb.AppendLine("</td>");

                            sb.AppendLine("<td>");
                            sb.Append($"<div class='refTempLabel' style='color: blue;'>[{StringHelper.Scale(tRefMin)}]</div>");
                            sb.AppendLine("</td>");

                            sb.AppendLine("</tr>"); // night

                            sb.AppendLine("</table>"); // Temperatures table


                            // Snow layer and risks info if any ...
                            sb.AppendLine("<tr>");
                            sb.AppendLine("<td colspan='2'>");

                            // Snow and risks table
                            sb.AppendLine("<table cellpadding=0 cellspacing=0 width='100%'>");

                            // Snow layer if any ...
                            sb.AppendLine("<tr>");
                            sb.AppendLine("<td>");

                            if (snow >= 300)
                                sb.AppendLine($"<div class='extraLabel' style='color: teal;'>Total snow cover: >300 cm</div>");
                            else if (snow > 0)
                                sb.AppendLine($"<div class='extraLabel' style='color: teal;'>Total snow cover: {snow} cm</div>");

                            // Risks if any ...
                            if (risks.Count > 0)
                            {
                                //sb.AppendLine($"<div class='extraLabel' style='color: red;'>{EnumRisks(risks)}</div>");
                                //phenomena.AddPhenomena(risks);
                            }

                            sb.AppendLine("</td>");
                            sb.AppendLine("</tr>");

                            sb.AppendLine("</table>"); // Snow and risks table

                            sb.AppendLine("<td>");
                            sb.AppendLine("</tr>");

                            sb.AppendLine("</table>"); // Cell table
                            sb.AppendLine("</div>"); // Cell table

                            td.BgColor = cellColor;
                            td.InnerHtml = sb.ToString();

                            if (weekStart == null)
                                weekStart = dt;
                            else
                                weekEnd = dt;

                            dt = dt.AddDays(1);

                            //td.Attributes.Add("style", "vertical-align: top; text-align: justify; ");
                            td.Attributes.Add("style", $"border: {borderSize}px solid {borderColor};");
                            td.Attributes.Add("title", toolTip);
                            td.Attributes.Add("class", "collapsible");

                            tr.Cells.Add(td);
                        }
                        else
                        {
                            DateTime vdt = dt.AddDays(d - (int)dt.DayOfWeek);

                            string dtStr = vdt.ToString("ddd, dd-MMM-yyyy");

                            sb.AppendLine($"<table cellpadding=0 cellspacing=1 width='100%' height='100%'>");

                            // Date label
                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td colspan='2' style='background-color: {dayBackColor}; color: lightgray;'>");
                            sb.AppendLine($"<div class='dateLabel' style='color: {dayTextColor};'>{dtStr}</div");
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
                            td.Attributes.Add("class", "collapsible");
                            tr.Cells.Add(td);
                        }

                        #endregion
                    }
                }

                #region Set content for the week summary cell
                {
                    string weekStartStr = weekStart.GetValueOrDefault().ToString("dd-MM-yyyy");
                    string weekEndStr = weekEnd.GetValueOrDefault().ToString("dd-MM-yyyy");
                    string summaryHeader = $"{weekStartStr}..{weekEndStr}";
                    string cellColor = "#EFEFEF";
                    int intervalLength = (int)((weekEnd - weekStart).Value.TotalDays);

                    string borderColor = "black";
                    int borderSize = 1;

                    if (weekStart <= today && today <= weekEnd)
                    {
                        // Current week
                        borderColor = "teal";
                        borderSize = 5;
                    }

                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"<table cellpadding=0 cellspacing=1 width='100%' height='100%'>");

                    // Date label
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td style='background-color: black; color: white;'>");
                    sb.AppendLine($"<div class='dateLabelSummary' style='color: white;'>{summaryHeader}</div");
                    sb.AppendLine("</td>");
                    sb.AppendLine("</tr>"); // Date label

                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>");

                    // Temperatures table
                    sb.AppendLine("<table cellpadding='0' cellspacing='2'>");

                    // Day
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>");
                    sb.AppendLine($"<img src='Images/day.png'/>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("<td>");
                    sb.Append($"<div class='actualTempLabelSummary' style='color: red;'>{stats_tMax}</div>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("<td>");
                    sb.Append($"<div class='refTempLabelSummary' style='color: red;'>[{stats_tRefMax}]</div>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("</tr>");

                    // Night
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>");
                    sb.AppendLine($"<img src='Images/night.png'/>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("<td>");
                    sb.Append($"<div class='actualTempLabelSummary' style='color: blue;'>{stats_tMin}</div>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("<td>");
                    sb.Append($"<div class='refTempLabelSummary' style='color: blue;'>[{stats_tRefMin}]</div>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("</table>"); // Temperatures table


                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>");
                    sb.AppendLine($"<div  class='actualTempLabelSummary'>{phenomena.BuildPhenomenaReport(intervalLength)}</div>");
                    sb.AppendLine("</td>");
                    sb.AppendLine("</tr>");

                    if (stats_snow.IsZero == false)
                    {
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td>");
                        sb.AppendLine($"<div class='actualTempLabelSummary'>Snow cover: {stats_snow.Report()} cm</div>");
                        sb.AppendLine("</td>");
                        sb.AppendLine("</tr>");
                    }

                    string tempProfile = "Seasonable temperatures";

                    bool bgSet = false;
                    if (!bgSet && stats_tMax.Max >= hot)
                    {
                        // heat wave
                        bgSet = true;
                        tempProfile = "Heat wave";
                        cellColor = "lightpink";
                    }

                    if ((stats_tMax.Min <= frost || stats_tMin.Min <= frost))
                    {
                        // Frosty
                        bgSet = true;
                        tempProfile = "Frosty";
                        cellColor = "lightblue";
                    }

                    if (!bgSet && stats_tMax.Avg > (stats_tRefMax.Avg + warmer))
                    {
                        // Much warmer than normal
                        bgSet = true;
                        tempProfile = "Much warmer than normal";
                        cellColor = "#FFCCAA";
                    }

                    if (!bgSet && stats_tMax.Avg > (stats_tRefMax.Avg + warm))
                    {
                        // A little warmer than normal
                        bgSet = true;
                        tempProfile = "A little warmer than normal";
                        cellColor = "#FFFF99";
                    }

                    if (!bgSet && stats_tMax.Avg < (stats_tRefMax.Avg + colder))
                    {
                        // Way colder than normal
                        bgSet = true;
                        tempProfile = "Much colder than normal";
                        cellColor = "powderblue";
                    }

                    if (!bgSet && stats_tMax.Avg < (stats_tRefMax.Avg + cold))
                    {
                        // Colder than normal
                        bgSet = true;
                        tempProfile = "A little colder than normal";
                        cellColor = "lightcyan";
                    }

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>");
                    sb.AppendLine($"<div class='actualTempLabelSummary'>{tempProfile}</div>");
                    sb.AppendLine($"</td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("</table>");

                    weekSummaryCell.InnerHtml = sb.ToString();
                    weekSummaryCell.Attributes.Add("style", $"border: {borderSize}px solid {borderColor}; background-color: {cellColor}; color: black; ");
                }
                #endregion

                _tblCalendar.Rows.Add(tr);

                HtmlTableRow sep2 = new HtmlTableRow();
                sep2.Attributes.Add("style", "height: 3px;");
                _tblCalendar.Rows.Add(sep2);
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
    }

    public static class Extensions
    {
        public static void AddPhenomenon(this Dictionary<string, int> list, string p, int intensity)
        {
            if (p.Contains("fog"))
            {
                if (p == "fog" && intensity < 3)
                    list.AddPhenomenon("sun", 0);
                else
                    p = "fog";
            }

            if (p.Contains("rain"))
                p = "rain";

            if (p.Contains("mix"))
                p = "sleet";

            if (p.Contains("ice"))
                p = "ice";

            if (p.Contains("inst"))
                p = "thunder";

            p = p.ToLowerInvariant();
            if (list.ContainsKey(p))
            {
                list[p] = list[p] + 1;
            }
            else
            {
                list.Add(p, 1);
            }
        }

        public static void AddPhenomena(this Dictionary<string, int> list, List<string> p)
        {
            p.ForEach((s) => list.AddPhenomenon(s, 0));
        }

        public static string BuildPhenomenaReport(this Dictionary<string, int> list, int intervalLength)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    KeyValuePair<string, int> thisElem = list.ElementAt(i);
                    KeyValuePair<string, int>? nextElem = null;

                    if (i < list.Count - 1)
                        nextElem = list.ElementAt(i + 1);

                    if (nextElem == null && thisElem.Key == "sun" && thisElem.Value >= 6)
                        sb.Append("strong sun; ");
                    else if (thisElem.Value < intervalLength - 4)
                        sb.Append($"some {thisElem.Key}; ");
                    else
                        sb.Append($"{thisElem.Key}; ");
                }
            }
            catch
            {
            }

            try
            {
                char[] chars = sb.ToString().Trim(";, ".ToCharArray()).ToCharArray();
                chars[0] = char.ToUpperInvariant(chars[0]);
                string retVal = new string(chars);
                return retVal.Replace(";", ",");
            }
            catch
            {
            }

            return sb.ToString();
        }   
    }

    public class WeekStats
    {
        public int Min { get; private set; }

        public int Max { get; private set; }

        public int Avg { get; private set; }

        public bool IsZero
        {
            get
            {
                return (Min == Max && Min == 0);
            }
        }

        List<int> _data = new List<int>();

        public void Add(int x)
        {
            _data.Add(x);

            if (x > Max)
                Max = x;
            if (x < Min)
                Min = x;

            int sum = 0;
            _data.ForEach((d) => sum += d);
            Avg = (int)Math.Round((float) sum / _data.Count);
        }

        public WeekStats()
        {
            this.Min = int.MaxValue;
            this.Max = int.MinValue;
        }

        public override string ToString()
        {
            if (Min != Max)
                return $"{StringHelper.Scale(Min)}..{ StringHelper.Scale(Max)}";

            return $"approx {StringHelper.Scale(Min)}";
        }

        public string Report()
        {
            if (Min != Max)
                return $"{Min}..{Max}";

            return $"approx {Min}";
        }
    }
}