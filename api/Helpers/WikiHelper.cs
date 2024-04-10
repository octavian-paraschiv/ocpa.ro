using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using System.IO;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers
{
    public interface IWikiHelper
    {
        string DefaultResponse { get; }
        Task<string> ProcessWikiFile(string wikiResourcePath);
    }

    public class WikiHelper : IWikiHelper
    {
        private IWebHostEnvironment _hostingEnvironment = null;
        static readonly string _defaultResponse = "<html><h1>NOT FOUND</h1><html>";

        public WikiHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string DefaultResponse => _defaultResponse;

        public async Task<string> ProcessWikiFile(string wikiResourcePath)
        {
            string html = DefaultResponse;
                    
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
                            .UseAdvancedExtensions()
                            .Build();

                        html = Markdown.ToHtml(markdown, pipeline);
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
