using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IContentRendererService
{
    Task<(byte[], bool)> RenderContent(string resourcePath, string reqRoot, string language, bool asHtml);
}