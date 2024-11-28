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
        private readonly object _dataLock = new object();

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IHttpClientFactory factory)
            : base(hostingEnvironment, logger)
        {
            _client = factory.CreateClient("caas");
            _fetchCompleted = new ManualResetEventSlim(false);
        }

        public byte[] GetNewCat(string resourcePath, string queryString)
        {
            byte[] dataToReturn = null;

            lock (_dataLock) { dataToReturn = _data; }

            _fetchCompleted.Reset();

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                var data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                lock (_dataLock) { _data = data; }
                _fetchCompleted.Set();
            });

            if (dataToReturn == null)
            {
                _fetchCompleted.Wait();
                lock (_dataLock) { _data = dataToReturn; }
            }

            return dataToReturn;
        }
    }
}

