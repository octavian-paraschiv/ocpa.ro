using Markdig;
using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface IWikiHelper
    {
        Task<byte[]> ProcessWikiResource(string wikiResourcePath);
    }

    public class WikiHelper : IWikiHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment = null;

        public WikiHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<byte[]> ProcessWikiResource(string wikiResourcePath)
        {
            byte[] data = null;
            try
            {
                wikiResourcePath = Path.Combine(_hostingEnvironment.ContentPath(), $"wiki/{wikiResourcePath}");

                if (File.Exists(wikiResourcePath))
                {
                    if (wikiResourcePath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
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

                            var body = Markdown.ToHtml(markdown, pipeline);
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
            catch
            {
                data = null;
            }

            return data;
        }
    }
}

