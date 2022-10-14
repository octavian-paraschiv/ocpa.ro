using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;


namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProToneController : ApiControllerBase
    {
        static readonly Version transitionVersion = new Version("3.1.59");

        public ProToneController(IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
        }

        [HttpGet]
        [Route("content")]
        public string GetContentPath()
        {
            return ContentPath;
        }

        [HttpGet("v1")]
        public IActionResult OldGetV1([FromQuery] string release = null, [FromQuery] string version = null)
        {
            try
            {
                Version v = null;

                if (!string.IsNullOrEmpty(version))
                {
                    v = new Version(version);
                    if (v < transitionVersion)
                    {
                        // older API's were sending app version in query string 
                        // Force all these apps to upgrade to 3.1.59 which is the transition build
                        return Ok(transitionVersion.ToString());
                    }
                }

                List<BuildInfo> builds = FetchBuilds(v, release);
                return Ok(JsonConvert.SerializeObject(builds));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string release = null, [FromQuery] string version = null)
        {
            try
            {
                Version v = null;

                if (!string.IsNullOrEmpty(version))
                {
                    v = new Version(version);
                    if (v < transitionVersion)
                    {
                        // older API's were sending app version in query string 
                        // Force all these apps to upgrade to 3.1.59 which is the transition build
                        return Ok(transitionVersion.ToString());
                    }
                }

                List<BuildInfo> builds = FetchBuilds(v, release);
                return Ok(builds);
            }
			catch (Exception ex)
            {
				return BadRequest(ex.Message);
            }
        }

        private List<BuildInfo> FetchBuilds(Version v, string release = "")
        {
            List<BuildInfo> builds = new List<BuildInfo>();

            if (string.IsNullOrEmpty(release))
                release = "false";

            switch (release.ToLowerInvariant())
            {
                case "false":
                    builds.AddRange(GetProtoneBuilds(BuildType.Experimental, v));
                    break;

                case "true":
                    builds.AddRange(GetProtoneBuilds(BuildType.Release, v));
                    break;

                case "legacy":
                    builds.AddRange(GetProtoneBuilds(BuildType.Legacy, v));
                    break;

                case "all":
                    builds.AddRange(GetProtoneBuilds(BuildType.Release, v));
                    builds.AddRange(GetProtoneBuilds(BuildType.Experimental, v));
                    break;
            }

            return builds;
        }

        private List<BuildInfo> GetProtoneBuilds(BuildType buildType, Version minVersion)
        {
            List<BuildInfo> list = new List<BuildInfo>();

            string folder = (buildType == BuildType.Legacy) ? "legacy" : "current";
            string path = System.IO.Path.Combine(ContentPath, $"ProTONE/{folder}");

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

            // Filter builds (keep only those with a higher version) and sort by version
            var ret = (from build in list
                       where (minVersion == null || minVersion < build.Version)
                       orderby build.Version ascending
                       select build);

            return ret.ToList();
        }

        private BuildInfo ReadBuildInfo(string path)
        {
            BuildInfo bi = null;

            try
            {
                if (System.IO.File.Exists(path))
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

                        // TODO this should eventually come from Digi Storage
                        URL = $"{Request.Scheme}://{Request.Host}/content/ProTONE/{folder}/{fileName}"
                    };

                    string infoFile = Path.ChangeExtension(path, "buildinfo.txt");
                    if (System.IO.File.Exists(infoFile))
                    {
                        string dts = System.IO.File.ReadAllText(infoFile);

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
                        System.IO.File.CreateText(infoFile).Close();
                    }
                }
            }
            catch
            {
            }

            return bi;
        }
    }
}