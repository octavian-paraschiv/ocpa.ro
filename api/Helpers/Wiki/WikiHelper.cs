using Markdig;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface IWikiHelper
    {
        string DefaultResponse { get; }
        Task<string> ProcessWikiFile(string wikiResourcePath);
    }

    public class WikiHelper : IWikiHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment = null;
        private static readonly string _defaultResponse = "<html><h1>NOT FOUND</h1><html>";

        public WikiHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string DefaultResponse => _defaultResponse;

        public async Task<string> ProcessWikiFile(string wikiResourcePath)
        {
            string html = DefaultResponse.Replace("NOT FOUND", $"{wikiResourcePath}: NOT FOUND");

            try
            {
                if (!wikiResourcePath.EndsWith(".md"))
                    wikiResourcePath += ".md";

                string rootPath = Path.GetDirectoryName(_hostingEnvironment.ContentRootPath);
                wikiResourcePath = Path.Combine(rootPath, $"Content/wiki/{wikiResourcePath}");


                if (File.Exists(wikiResourcePath))
                {
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
                        html = $"<html>" +
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
                    }
                }
            }
            catch
            {
            }

            return html;
        }
    }
}

