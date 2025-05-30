using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Configuration;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Content
{
    public interface ICaasHelper
    {
        Task RefreshCat(string resourcePath, string queryString);
    }

    public class CaasHelper : BaseHelper, ICaasHelper
    {
        private const string DefaultCatImagePath = "wiki/daily-cat/daily-cat.gif";
        private readonly HttpClient _client;

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IHttpClientFactory factory, IDistributedCache cache, IOptions<CaasConfig> config)
            : base(hostingEnvironment, logger)
        {
            _client = factory.CreateClient("caas");
        }

        public async Task RefreshCat(string resourcePath, string queryString)
        {
            try
            {
                _logger.Debug($"[CAAS] Fetching new image ...");

                var data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                if (!(data?.Length > 0))
                {
                    var imagePath = Path.Combine(_hostingEnvironment.ContentPath(), DefaultCatImagePath);
                    await File.WriteAllBytesAsync(imagePath, data);

                    _logger.Debug($"[CAAS] All fetching OK");
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
    }
}

