using Markdig;
using ocpa.ro.application.Renderers;
using ocpa.ro.domain.Abstractions.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.application.Services;

public class ContentRendererService : BaseService, IContentRendererService
{
    private List<ContentRendererBase> _renderers;

    public ContentRendererService(IHostingEnvironmentService hostingEnvironment, ILogger logger)
        : base(hostingEnvironment, logger)
    {
        _renderers = typeof(ContentRendererService).Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ContentRendererBase)))
            .Select(t => Activator.CreateInstance(t) as ContentRendererBase)
            .ToList();
    }

    public string ContentPath => _hostingEnvironment.ContentPath;

    public async Task<(byte[], bool)> RenderContent(string resourcePath, string reqRoot, string language, bool asHtml)
    {
        byte[] data = null;

        try
        {
            bool resourceExists = false;
            var origPath = Path.Combine(_hostingEnvironment.ContentPath, resourcePath);
            var origExt = Path.GetExtension(origPath);
            var localizedResourcePath = Path.ChangeExtension(origPath, $".{language}{origExt}");

            if (File.Exists(localizedResourcePath))
            {
                // Localized Wiki resource exists, use it
                resourcePath = localizedResourcePath;
                resourceExists = true;
            }
            else
            {
                // Localized Wiki resource does not exist, check whether the default one exists
                resourcePath = Path.Combine(_hostingEnvironment.ContentPath, resourcePath);
                resourceExists = File.Exists(resourcePath);
            }

            if (resourceExists)
            {
                if (resourcePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    // Markdown file
                    var markdown = await File.ReadAllTextAsync(resourcePath).ConfigureAwait(false);
                    if (markdown?.Length > 0)
                    {
                        var pageDirName = Path.GetRelativePath(_hostingEnvironment.ContentPath,
                            Directory.GetParent(resourcePath).FullName)
                            .Replace(_hostingEnvironment.ContentPath, string.Empty)
                            .Replace("\\", "/")
                            .Trim('/');

                        string body = RenderCustomCodeBlocks(markdown);

                        body = body
                            .Replace("%root%", $"{reqRoot.TrimEnd('/')}/Content/render")
                            .Replace("%wiki%", $"{reqRoot.TrimEnd('/')}/Content/render/wiki")
                            .Replace("%page%", $"{reqRoot.TrimEnd('/')}/Content/render/{pageDirName}");

                        if (asHtml)
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

                        if (asHtml)
                        {
                            sb.AppendLine("<html><head><meta charset='utf-8'><meta http-equiv='cache-control' content='no-cache'>");
                            sb.AppendLine("<style>.markdown-body {{ font-family: Arial; font-size: 12px; line-height: 1.3; word-wrap: break-word; }}</style>");
                            sb.AppendLine("<script src='https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_SVG' defer></script>");
                            sb.AppendLine("</head><body><div class='markdown-body'>");
                        }

                        sb.AppendLine(body);

                        if (asHtml)
                            sb.AppendLine("</div></body><html>");

                        return (Encoding.UTF8.GetBytes(sb.ToString()), false);
                    }
                }
                else
                {
                    // Static content file
                    data = await File.ReadAllBytesAsync(resourcePath).ConfigureAwait(false);
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

