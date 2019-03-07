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
                    string response = "{}";
                    try
                    {
                        bool isRelease = false;

                        var release = Request.QueryString["release"];
                        var version = Request.QueryString["version"];

                        if (string.IsNullOrEmpty(release))
                            release = "false";

                        List<BuildInfo> builds = new List<BuildInfo>();

                        if (string.IsNullOrEmpty(version))
                        {
                            // Newer API
                            switch (release.ToLowerInvariant())
                            {
                                case "false":
                                    builds.AddRange(GetProtoneBuilds(BuildType.Experimental));
                                    break;

                                case "all":
                                    builds.AddRange(GetProtoneBuilds(BuildType.Release));
                                    builds.AddRange(GetProtoneBuilds(BuildType.Experimental));
                                    break;

                                case "true":
                                default:
                                    builds.AddRange(GetProtoneBuilds(BuildType.Release));
                                    break;
                            }

                            response = JsonConvert.SerializeObject(builds, Formatting.Indented);
                        }
                        else
                        {
                            // older API's were sending app version in query string 
                            // Force all these apps to upgrade to 3.1.59 which is the transition build
                            response = "3.1.59";
                        }
                    }
                    catch
                    {
                    }

                    Response.Write(response);
                    Response.End();
                    return;
                }

                ListProtoneVersions(tblProtoneCurrent, BuildType.Release);
                ListProtoneVersions(tblProtoneVersions, BuildType.Legacy);
                ListProtoneVersions(tblExperimental, BuildType.Experimental);
            }
        }

        private void ListProtoneVersions(HtmlTable table, BuildType buildType)
        {
            table.Rows.Clear();

            var builds = GetProtoneBuilds(buildType);

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

        public List<BuildInfo> GetProtoneBuilds(BuildType buildType)
        {
            List<BuildInfo> list = new List<BuildInfo>();

            string folder = (buildType == BuildType.Legacy) ? "legacy" : "current";
            string path = Path.Combine(Request.PhysicalApplicationPath, folder);
            if (string.IsNullOrEmpty(path) == false)
            {
                var files = Directory.GetFiles(path, "*.exe");
                if (files != null && files.Length > 0)
                {
                    var fileList = files.ToList();
                    foreach (var file in fileList)
                    {
                        BuildInfo bi = ReadBuildInfo(file);
                        if (bi != null)
                        {
                            switch (buildType)
                            {
                                case BuildType.Legacy:
                                    list.Add(bi);
                                    break;

                                case BuildType.Release:
                                    if (bi.IsRelease)
                                        list.Add(bi);
                                    break;

                                case BuildType.Experimental:
                                    if (!bi.IsRelease)
                                        list.Add(bi);
                                    break;
                            }
                        }
                    }
                }
            }

            list.Sort((l1, l2) =>
            {
                return l2.Version.CompareTo(l1.Version);
            });

            return list;
        }

        private BuildInfo ReadBuildInfo(string path)
        {
            BuildInfo bi = null;

            try
            {
                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);
                    string folder = fi.Directory.Name;
                    string fileName = Path.GetFileName(path);
                    string fileTitle = Path.GetFileNameWithoutExtension(path);

                    string vs = fileTitle.Replace("ProTONE Suite", "").Trim();
                   
                    bi = new BuildInfo
                    {
                        Title = fileTitle,
                        Version = new Version(vs),
                        URL = $"{Request.Url.Scheme}://{Request.Url.Host}/ProTONE/{folder}/{fileName}"
                    };
                    
                    string infoFile = Path.ChangeExtension(path, "buildinfo.txt");
                    if (File.Exists(infoFile))
                    {
                        string dts = File.ReadAllText(infoFile);

                        string[] fields = dts.Split(',');
                        if (fields != null && fields.Length > 0)
                        {
                            DateTimeConverter dtc = new DateTimeConverter();
                            bi.BuildDate = (DateTime)dtc.ConvertFromInvariantString(fields[0]);

                            if (fields.Length > 1)
                            {
                                BooleanConverter bc = new BooleanConverter();
                                bi.IsRelease = (bool)bc.ConvertFromInvariantString(fields[1]);

                                if (fields.Length > 2)
                                {
                                    bi.Comment = fields[2];
                                }
                            }
                        }

                    }
                    else
                    {
                        File.CreateText(infoFile).Close();
                    }
                }
            }
            catch
            {
            }

            return bi;
        }

    }

    [Flags]
    public enum BuildType
    {
        Legacy,
        Experimental,
        Release,
    }

    [JsonObject]
    public class BuildInfo
    {
        public string Title { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public DateTime BuildDate { get; set; }

        public bool IsRelease { get; set; }

        public string Comment { get; set; }

        public string URL { get; set; }
    }

    [JsonObject]
    public class Builds
    {
        public BuildInfo[] List;
    }
}