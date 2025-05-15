using Markdig;
using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Wiki.CustomRenderers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface IWikiHelper
    {
        Task<(byte[], bool)> ProcessWikiResource(string wikiResourcePath, string reqRoot, string language,
            bool fullHtml);
    }

    public class WikiHelper : BaseHelper, IWikiHelper
    {
        private List<CustomRendererBase> _renderers;

        public WikiHelper(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger)
        {
            _renderers = typeof(WikiHelper).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CustomRendererBase)))
                .Select(t => Activator.CreateInstance(t) as CustomRendererBase)
                .ToList();
        }

        public async Task<(byte[], bool)> ProcessWikiResource(string wikiResourcePath, string reqRoot, string language,
            bool renderAsHtml)
        {
            byte[] data = null;

            try
            {
                bool resourceExists = false;
                var origPath = Path.Combine(_hostingEnvironment.ContentPath(), wikiResourcePath);
                var origExt = Path.GetExtension(origPath);
                var localizedResourcePath = Path.ChangeExtension(origPath, $".{language}{origExt}");

                if (File.Exists(localizedResourcePath))
                {
                    // Localized Wiki resource exists, use it
                    wikiResourcePath = localizedResourcePath;
                    resourceExists = true;
                }
                else
                {
                    // Localized Wiki resource does not exist, check whether the default one exists
                    wikiResourcePath = Path.Combine(_hostingEnvironment.ContentPath(), wikiResourcePath);
                    resourceExists = File.Exists(wikiResourcePath);
                }

                if (resourceExists)
                {
                    if (wikiResourcePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        // Markdown file
                        var markdown = await File.ReadAllTextAsync(wikiResourcePath).ConfigureAwait(false);
                        if (markdown?.Length > 0)
                        {
                            var pageDirName = Path.GetRelativePath(_hostingEnvironment.ContentPath(),
                                Directory.GetParent(wikiResourcePath).FullName)
                                .Replace(_hostingEnvironment.ContentPath(), string.Empty)
                                .Replace("\\", "/")
                                .Trim('/');

                            string body = RenderCustomCodeBlocks(markdown);

                            body = body
                                .Replace("%root%", $"{reqRoot.TrimEnd('/')}/Content/render")
                                .Replace("%wiki%", $"{reqRoot.TrimEnd('/')}/Content/render/wiki")
                                .Replace("%page%", $"{reqRoot.TrimEnd('/')}/Content/render/{pageDirName}");

                            if (renderAsHtml)
                            {
                                var pipeline = new MarkdownPipelineBuilder()
                                   .UseBootstrap()
                                   .UseEmojiAndSmiley()
                                   .UseSoftlineBreakAsHardlineBreak()
                                   .UseAdvancedExtensions()
                                   .Build();

                                body = Markdown.ToHtml(body, pipeline);
                            }

                            StringBuilder sb = new();

                            if (renderAsHtml)
                            {
                                sb.AppendLine("<html><head><meta charset='utf-8'><meta http-equiv='cache-control' content='no-cache'>");
                                sb.AppendLine("<style>.markdown-body {{ font-family: Arial; font-size: 12px; line-height: 1.3; word-wrap: break-word; }}</style>");
                                sb.AppendLine("<script src='https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_SVG' defer></script>");
                                sb.AppendLine("</head><body><div class='markdown-body'>");
                            }

                            sb.AppendLine(body);

                            if (renderAsHtml)
                                sb.AppendLine("</div></body><html>");

                            return (Encoding.UTF8.GetBytes(sb.ToString()), false);
                        }
                    }
                    else
                    {
                        // Static content file
                        data = await File.ReadAllBytesAsync(wikiResourcePath).ConfigureAwait(false);
                        return (data, true);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return (null, false);
        }

        private string RenderCustomCodeBlocks(string body)
        {
            try
            {
                if (_renderers?.Count > 0)
                {
                    foreach (var renderer in _renderers)
                        body = renderer.RenderBody(body);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return body;
        }
    }
}

