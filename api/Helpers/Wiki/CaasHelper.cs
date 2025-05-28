using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Models.Configuration;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface ICaasHelper
    {
        Task<byte[]> GetNewCat(string resourcePath, string queryString, bool refreshCat);
    }

    public class CaasHelper : BaseHelper, ICaasHelper
    {
        private const string CatCacheKey = "cat";

        private const string DefaultCatImagePath = "Helpers/Wiki/loadcat.gif";

        private readonly CaasConfig _config;
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IHttpClientFactory factory, IDistributedCache cache, IOptions<CaasConfig> config)
            : base(hostingEnvironment, logger)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _client = factory.CreateClient("caas");
        }

        public async Task<byte[]> GetNewCat(string resourcePath, string queryString, bool refreshCat)
        {
            byte[] data = null;

            try
            {
                data = await _cache.GetAsync(CatCacheKey);

                if (!(data?.Length > 0))
                {
                    refreshCat = true;
                    data = await File.ReadAllBytesAsync(DefaultCatImagePath);
                }

                if (refreshCat)
                {
                    ThreadPool.QueueUserWorkItem(async _ =>
                    {
                        var queryDict = QueryHelpers.ParseQuery(queryString ?? string.Empty);
                        if (queryDict.ContainsKey(nameof(refreshCat)))
                            queryDict.Remove(nameof(refreshCat));

                        queryString = QueryString.Create(queryDict).Value;

                        var data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                        if (data?.Length > 0)
                        {
                            try
                            {
                                _logger.Debug($"[CAAS] New image fetched OK, caching it ...");
                                await _cache.SetAsync(CatCacheKey, data, new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CachePeriod)
                                });
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }

                            try
                            {
                                _logger.Debug($"[CAAS] Saving image on disk ...");
                                await File.WriteAllBytesAsync(DefaultCatImagePath, data);
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }

                            _logger.Debug($"[CAAS] All fetching OK");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return data;
        }
    }
}

