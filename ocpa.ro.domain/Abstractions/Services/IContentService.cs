using ocpa.ro.domain.Models.Content;
using System.Net;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IContentService
{
    public Task<byte[]> GetContent(string contentPath);

    public ContentUnit ListContent(string contentPath, int? level, string filter, bool markdownView);

    public UpdatedContentUnit MoveContent(string contentPath, string newPath);

    public UpdatedContentUnit CreateNewFolder(string contentPath);

    public Task<UpdatedContentUnit> CreateOrUpdateContent(string contentPath, byte[] contentBytes);

    public HttpStatusCode DeleteContent(string contentPath);

    public string LatestThorusStudioFile { get; }
}