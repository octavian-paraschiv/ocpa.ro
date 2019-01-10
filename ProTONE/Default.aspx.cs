using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.ComponentModel;

namespace ProTONE
{
    public partial class Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ListProtoneVersions(tblProtoneCurrent, "release");
                ListProtoneVersions(tblProtoneVersions, "legacy");
                ListProtoneVersions(tblExperimental, "current");
            }
        }

        private void ListProtoneVersions(HtmlTable table, string folder)
        {
            table.Rows.Clear();

            var path = Path.Combine(Request.PhysicalApplicationPath, folder);
            var highlightLatest = (folder == "release");

            var files = Directory.GetFiles(path, "*.exe");
            if (files != null)
            {
                var list = files.ToList();

                list.Sort(Sorter);

                bool isLatestSet = false;

                foreach (string file in list)
                {
                    string fileName = Path.GetFileName(file);
                    string fileTitle = Path.GetFileNameWithoutExtension(file);
                    var buildDateTime = ReadBuildDate(file);
                    var dtStr = buildDateTime.ToString("ddd, dd-MMM-yyyy");

                    HtmlTableRow row = new HtmlTableRow();

                    string hdr = "h4";
                    string style = "margin: 0px; padding-bottom: 5px;";

                    HtmlTableCell nameCell = new HtmlTableCell();
                    HtmlTableCell descCell = new HtmlTableCell();

                    if (isLatestSet || highlightLatest == false)
                    {
                        nameCell.InnerHtml = $"<{hdr} style='{style}'><a href='{folder}/{fileName}'>{fileTitle}</a></{hdr}>";
                        descCell.InnerHtml = $"<{hdr} style='{style}'>[Built on: {dtStr}]</{hdr}>";
                    }
                    else
                    {
                        isLatestSet = true;
                        string latestStyle = "margin: 0px; padding-bottom: 10px; font-weight: bold;";
                        nameCell.InnerHtml = $"<{hdr} style='{latestStyle}'><a href='{folder}/{fileName}'>{fileTitle}</a></{hdr}>";
                        descCell.InnerHtml = $"<{hdr} style='{latestStyle}'>[Latest - Built on: {dtStr}]</{hdr}>";
                    }


                    row.Cells.Add(nameCell);
                    row.Cells.Add(descCell);
                    table.Rows.Add(row);
                }
            }
        }

        private int Sorter(string f1, string f2)
        {
            try
            {
                string vs1 = Path.GetFileNameWithoutExtension(f1).Replace("ProTONE Suite", "").Trim();
                string vs2 = Path.GetFileNameWithoutExtension(f2).Replace("ProTONE Suite", "").Trim();

                Version v1 = new Version(vs1);
                Version v2 = new Version(vs2);

                // Sort newest on top of oldest
                return v2.CompareTo(v1);
            }
            catch
            {
            }

            return 0;
        }

        private static DateTime ReadBuildDate(string path)
        {
            var epoch = new DateTime(1970, 1, 1);
            DateTime buildDateTime = epoch;

            try
            {
                if (File.Exists(path))
                {
                    string infoFile = Path.ChangeExtension(path, "buildinfo.txt");
                    if (File.Exists(infoFile))
                    {
                        string dts = File.ReadAllText(infoFile);

                        DateTimeConverter dtc = new DateTimeConverter();
                        buildDateTime = (DateTime)dtc.ConvertFromInvariantString(dts);
                    }
                    else
                    {
                        File.CreateText(infoFile).Close();
                    }
                }
            }
            catch { }

            return buildDateTime;
        }

    }
}