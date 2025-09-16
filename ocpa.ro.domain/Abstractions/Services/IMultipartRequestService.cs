using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Services;
public interface IMultipartRequestService
{
    Task<byte[]> GetMultipartRequestData(HttpRequest request);
}