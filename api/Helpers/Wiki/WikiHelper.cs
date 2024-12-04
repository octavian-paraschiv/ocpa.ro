using Markdig;
using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface IWikiHelper
    {
        Task<byte[]> ProcessWikiResource(string wikiResourcePath, string reqRoot, string language);
    }

    public class WikiHelper : BaseHelper, IWikiHelper
    {
        public WikiHelper(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger)
        {
        }

        public async Task<byte[]> ProcessWikiResource(string wikiResourcePath, string reqRoot, string language)
        {
            byte[] data = null;
            try
            {
                bool resourceExists = false;
                var localizedResourcePath = Path.Combine(_hostingEnvironment.ContentPath(), $"wiki/{language}.{wikiResourcePath}");

                if (File.Exists(localizedResourcePath))
                {
                    // Localized Wiki resource exists, use it
                    wikiResourcePath = localizedResourcePath;
                    resourceExists = true;
                }
                else
                {
                    // Localized Wiki resource does not exist, check whether the default one exists
                    wikiResourcePath = Path.Combine(_hostingEnvironment.ContentPath(), $"wiki/{wikiResourcePath}");
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
                            var pipeline = new MarkdownPipelineBuilder()
                                .UseBootstrap()
                                .UseEmojiAndSmiley()
                                .UseSoftlineBreakAsHardlineBreak()
                                .UseAdvancedExtensions()
                                .Build();

                            var body = Markdown
                                .ToHtml(markdown, pipeline)
                                .Replace("%root%", $"{reqRoot.TrimEnd('/')}/wiki");

                            var html = $"<html>" +
                                $"<head>" +
                                $"<meta charset=\"utf-8\">" +
                                $"<meta http-equiv=\"cache-control\" content=\"no-cache\">" +
                                $"<style>" +
                                $".markdown-body {{ font-family: Arial; font-size: 12px; line-height: 1.3; word-wrap: break-word; }}" +
                                $"</style>" +
                                $"</head>" +
                                $"<body>" +
                                $"<div class=\"markdown-body\">" +
                                $"{body}" +
                                $"</div>" +
                                $"</body>" +
                                $"<html>";

                            return Encoding.UTF8.GetBytes(html);
                        }
                    }
                    else
                    {
                        // Static content file
                        data = await File.ReadAllBytesAsync(wikiResourcePath).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                data = null;
            }

            return data;
        }
    }
}

