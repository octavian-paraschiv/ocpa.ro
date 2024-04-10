using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers
{
    public interface IWikiHelper
    {
        string GetDefaultResponse();
        Task<string> ProcessMarkdownFile(string markdownResource);
    }

    public class WikiHelper : IWikiHelper
    {
        private IConfiguration _configuration = null;
        private IWebHostEnvironment _hostingEnvironment = null;

        public WikiHelper(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public string GetDefaultResponse() => "<html><h1>NOT FOUND</h1><html>";

        public async Task<string> ProcessMarkdownFile(string markdownResource)
        {
            string html = null;
                    
            try
            {
                // Configure the pipeline with all advanced extensions active
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                html = Markdown.ToHtml($"This is a text with some *{markdownResource}*", pipeline);
            }
            catch
            {
            }

            return html;
        }
    }
}
