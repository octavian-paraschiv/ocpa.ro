using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using common;
using System.Net.Http;
using System.Net;

namespace ProTONE
{
    public partial class Default : System.Web.UI.Page
    {
        List<string> _releases = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString != null && Request.QueryString.Count > 0)
                {
                    // Redirect to backend to keep backwards compatibility for ProTONE 3.4.x and lower
                    // ProTONE 3.5.2 and higher send version requests to http://ocpa.ro/backend/protone?version=x.y.z&release=(true|false)
                    // ProTONE 3.4.x and lower send version requests to http://ocpa.ro/protone&release=(true|false)
                    Response.Redirect(GetBackendUri());
                    return;
                }

                ListProtoneVersions(tblProtoneCurrent, BuildType.Release, "<b>[ No planned ProTONE releases! Choose a Legacy version from the list below. ]</b>");
                ListProtoneVersions(tblProtoneVersions, BuildType.Legacy, "<b>[ Something weird happened. There really should have been Legacy versions here. ]</b>");
                ListProtoneVersions(tblExperimental, BuildType.Experimental, "<b>[ No experimental ProTONE versions! Choose a Legacy version from the list above. ]</b>");
            }
        }

        private void ListProtoneVersions(HtmlTable table, BuildType buildType, string emptyContentHtml)
        {
            table.Rows.Clear();

            var builds = GetProtoneBuilds(buildType);

            if (builds == null || builds.Count < 1)
            {
                HtmlTableRow row = new HtmlTableRow();
                HtmlTableCell nameCell = new HtmlTableCell();
                nameCell.InnerHtml = emptyContentHtml;
                row.Cells.Add(nameCell);
                table.Rows.Add(row);
            }
            else
            {
                var highlightLatest = (buildType == BuildType.Release);
                bool isLatestSet = false;

                foreach (var build in builds)
                {
                    HtmlTableRow row = new HtmlTableRow();

                    string hdr = "h4";
                    string style = "margin: 0px; padding-bottom: 5px;";

                    HtmlTableCell nameCell = new HtmlTableCell();
                    HtmlTableCell descCell = new HtmlTableCell();

                    var dtStr = build.BuildDate.ToString("ddd, dd-MMM-yyyy");

                    if (isLatestSet || highlightLatest == false)
                    {
                        nameCell.InnerHtml = $"<{hdr} style='{style}'><a href='{build.URL}'>{build.Title}</a></{hdr}>";
                        descCell.InnerHtml = $"<{hdr} style='{style}'>[Built on: {dtStr}]</{hdr}>";
                    }
                    else
                    {
                        isLatestSet = true;
                        string latestStyle = "margin: 0px; padding-bottom: 10px; font-weight: bold;";
                        nameCell.InnerHtml = $"<{hdr} style='{latestStyle}'><a href='{build.URL}'>{build.Title}</a></{hdr}>";
                        descCell.InnerHtml = $"<{hdr} style='{latestStyle}'>[Latest - Built on: {dtStr}]</{hdr}>";
                    }


                    row.Cells.Add(nameCell);
                    row.Cells.Add(descCell);
                    table.Rows.Add(row);
                }
            }
        }

        private List<BuildInfo> GetProtoneBuilds(BuildType buildType)
        {
            List<BuildInfo> builds = new List<BuildInfo>();

            try
            {
                var reqUri = GetBackendUri();
                switch (buildType)
                {
                    case BuildType.Experimental:
                        reqUri += "?release=false";
                        break;

                    case BuildType.Legacy:
                        reqUri += "?release=legacy";
                        break;

                    case BuildType.Release:
                        reqUri += "?release=true";
                        break;
                }

                using (WebClient wc = new WebClient())
                {
                    string json = wc.DownloadString(reqUri);
                    builds = JsonConvert.DeserializeObject<List<BuildInfo>>(json);
                }
            }
            catch(Exception ex)
            {
                string s = ex.Message;
            }
            return builds;
        }

        private string GetBackendUri()
        {
            var reqUri = Request.Url.ToString().ToLowerInvariant();
            return reqUri.Replace("/protone/", "/backend/protone/");
        }
    }
}