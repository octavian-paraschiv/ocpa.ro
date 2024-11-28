using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface ICaasHelper
    {
        Task<byte[]> GetNewCat(string resourcePath, string queryString);
    }

    public class CaasHelper : BaseHelper, ICaasHelper
    {
        private const string CacheKey = "cat";
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IHttpClientFactory factory, IDistributedCache cache)
            : base(hostingEnvironment, logger)
        {
            _client = factory.CreateClient("caas");
            _cache = cache;
        }

        public async Task<byte[]> GetNewCat(string resourcePath, string queryString)
        {
            byte[] data = null;

            try
            {
                data = await _cache.GetAsync(CacheKey);

                if (data?.Length > 0)
                {
                    // Fetch new cat in advance
                    _ = FetchNewCatAsync(resourcePath, queryString).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                            LogException(task.Exception);
                    });

                    return data;
                }

                data = await FetchNewCatAsync(resourcePath, queryString);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return data;
        }

        private async Task<byte[]> FetchNewCatAsync(string resourcePath, string queryString)
        {
            byte[] data = null;

            try
            {
                data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                if (data?.Length > 0)
                    await _cache.SetAsync(CacheKey, data);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return data;
        }
    }
}

