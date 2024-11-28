using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Net.Http;
using System.Threading;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface ICaasHelper
    {
        byte[] GetNewCat(string resourcePath, string queryString);
    }

    public class CaasHelper : BaseHelper, ICaasHelper
    {
        private byte[] _data;
        private readonly HttpClient _client;
        private readonly ManualResetEventSlim _fetchCompleted;

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IHttpClientFactory factory)
            : base(hostingEnvironment, logger)
        {
            _client = factory.CreateClient("caas");
            _fetchCompleted = new ManualResetEventSlim(false);
        }

        public byte[] GetNewCat(string resourcePath, string queryString)
        {
            var dataToReturn = _data;

            _fetchCompleted.Reset();

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                _data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                _fetchCompleted.Set();
            });

            if (dataToReturn == null)
            {
                _fetchCompleted.Wait();
                dataToReturn = _data;
            }

            return dataToReturn;
        }
    }
}

