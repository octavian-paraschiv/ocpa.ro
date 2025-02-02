﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.ProTONE;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;


namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ProToneController : ApiControllerBase
    {
        #region Private members
        private static readonly BuildVersion transitionVersion = new("3.1.59");
        #endregion

        #region Constructor (DI)

        public ProToneController(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
        }

        #endregion

        #region Public controller methods

        [HttpGet("v1")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "LegacyGetBuilds")]
        public IActionResult LegacyGetBuilds([FromQuery] string release = null, [FromQuery] string version = null)
        {
            try
            {
                BuildVersion v = null;

                if (!string.IsNullOrEmpty(version))
                {
                    v = new BuildVersion(version);
                    if (v.LessThan(transitionVersion))
                    {
                        // older API's were sending app version in query string 
                        // Force all these apps to upgrade to 3.1.59 which is the transition build
                        return Ok(transitionVersion.ToString());
                    }
                }

                List<BuildInfo> builds = FetchBuilds(v, release);
                return Ok(JsonSerializer.Serialize(builds));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BuildInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetBuilds")]
        public IActionResult GetBuilds([FromQuery] string release = null, [FromQuery] string version = null)
        {
            try
            {
                BuildVersion v = null;

                if (!string.IsNullOrEmpty(version))
                {
                    v = new BuildVersion(version);
                    if (v.LessThan(transitionVersion))
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

        #endregion

        #region Private methods

        private List<BuildInfo> FetchBuilds(BuildVersion v, string release = "")
        {
            List<BuildInfo> builds = [];

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

        private IEnumerable<BuildInfo> GetProtoneBuilds(BuildType buildType, BuildVersion minVersion)
        {
            List<BuildInfo> list = [];
            string folder = "legacy";

            switch (buildType)
            {
                case BuildType.Experimental:
                    folder = "beta";
                    break;

                case BuildType.Release:
                    folder = "current";
                    break;

                case BuildType.Legacy:
                    folder = "legacy";
                    break;
            }

            string path = Path.Combine(_hostingEnvironment.ContentPath(), $"ProTONE/{folder}");

            if (path?.Length > 0 && Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.exe");
                if (files?.Length > 0)
                {
                    files.ToList().ForEach(file =>
                    {
                        BuildInfo bi = ReadBuildInfo(file);
                        list.Add(bi);
                    });
                }
            }

            // Filter builds (keep only those with a higher version) and sort by version
            var ret = (from build in list
                       where minVersion == null || minVersion.LessThan(build.Version)
                       orderby build.Version.ToString() ascending
                       select build);

            return ret;
        }

        private BuildInfo ReadBuildInfo(string path)
        {
            BuildInfo bi = null;

            try
            {
                if (System.IO.File.Exists(path))
                {
                    FileInfo fi = new(path);
                    string folder = fi.Directory.Name;
                    string fileName = Path.GetFileName(path);
                    string fileTitle = Path.GetFileNameWithoutExtension(path);

                    string vs = fileTitle.Replace("ProTONE Suite", "").Trim();

                    bi = new BuildInfo
                    {
                        Title = fileTitle,
                        Version = new BuildVersion(vs),
                        URL = $"{Request.Scheme}://{Request.Host}/content/ProTONE/{folder}/{fileName}"
                    };

                    string infoFile = Path.ChangeExtension(path, "buildinfo.txt");
                    if (System.IO.File.Exists(infoFile))
                    {
                        string dts = System.IO.File.ReadAllText(infoFile);

                        string[] fields = dts.Split(',');
                        if (fields.Length > 0)
                        {
                            DateTimeConverter dtc = new();
                            bi.BuildDate = (DateTime)dtc.ConvertFromInvariantString(fields[0]);

                            if (fields.Length > 1)
                            {
                                BooleanConverter bc = new();
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
            catch (Exception ex)
            {
                LogException(ex);
            }

            return bi;
        }

        #endregion
    }
}